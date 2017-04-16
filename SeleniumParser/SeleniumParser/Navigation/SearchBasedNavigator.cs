using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class SearchBasedNavigator : Navigator
    {
        static List<string> SearchTerms = new List<string> { 
            "Funny"
           , "Humor"
           , "sarcasm"
           , "punny"
           , "satire"
           , "pun"
           , "zombie"
           , "math"
           , "gym"
           , "motivation"
           , "beer"
           , "football"
#region old stuff
            //"cat t shirt"
            //, "motivational t shirt"
            //, "motivational exercise t shirt"
            //, "saying t shirt"
            //, "donald trump t shirt"
            //, "anti donald trump t shirt"
            //, "political t shirt"
            //, "cat saying t shirt"
            //, "dog saying t shirt"
            //, "funny saying t shirt"
            //, "french bulldog t shirt"
            //, "pug t shirt"
            //, "funny saying t shirt"
            //, "coffee t shirt"
            //, "tea t shirt"
            //, "doughnut t shirt"
            //, "salty t shirt"
            //, "snatched t shirt"
            //, "werq t shirt"
            //, "slay t shirt"
            //, "yolo t shirt"
            //, "a crapella t shirt"
            //, "amazeballs t shirt"
            //, "askhole t shirt"
            //, "awesome sauce t shirt"
            //, "baby bump t shirt"
            //, "badassery t shirt"
            //, "beer me t shirt"
            //, "blamestorming t shirt"
            //, "bootylicious t shirt"
            //, "bromance t shirt"
            //, "bye felicia t shirt"
            //, "cougar t shirt"
            //, "designated drunk t shirt"
            //, "duck face t shirt"
            //, "fanboi t shirt"
            //, "fan girl t shirt"
            //, "fauxpology t shirt"
            //, "foodie t shirt"
            //, "frak t shirt"
            //, "friend zone t shirt"
            //, "fro yo t shirt"
            //, "fro-yo t shirt"
            //, "gaydar t shirt"
            //, "girl crush t shirt"
            //, "grrrl t shirt"
            //, "hangry t shirt"
            //, "hipster t shirt"
            //, "hot mess t shirt"
            //, "humblebrag t shirt"
            //, "jailbait t shirt"
            //, "knosh t shirt"
            //, "lol t shirt"
            //, "make it rain t shirt"
            //, "man boobs t shirt"
            //, "man cave t shirt"
            //, "man sweats t shirt"
            //, "milf t shirt"
            //, "netflix and chill t shirt"
            //, "ninja sex t shirt"
            //, "nom t shirt"
            //, "nontroversy t shirt"
            //, "nsfw t shirt"
            //, "party foul t shirt"
            //, "phat t shirt"
            //, "pregret t shirt"
            //, "pwned t shirt"
            //, "quantum physics t shirt"
            //, "ratchet t shirt"
            //, "rendezbooz t shirt"
            //, "rickroll t shirt"
            //, "said no one ever t shirt"
            //, "selfie t shirt"
            //, "sext t shirt"
            //, "side boob t shirt"
            //, "that's what she said t shirt"
            //, "trout t shirt"
            //, "twerk t shirt"
            //, "typeractive t shirt"
            //, "wtf t shirt"
            //, "yolo t shirt"
            //, "zombie t shirt"
            //, "sus t shirt"
            //, "boots t shirt"
            //, "hunty t shirt"
            //, "savage t shirt"
            //, "thirsty t shirt"
            //, "Hillary t shirt"
            //, "blow cup t shirt"
#endregion
        };

        SearcherFactory.SearcherType ProductSearcher;

        public SearchBasedNavigator(
            SearcherFactory.SearcherType searcher
            , List<string> categoriesToConsider
            , int maxBsr = 100000
            , int numberOfProductsToFind = 5
            , int numberOfPagesToSearch = 20
            )
            : base(
            categoriesToConsider
            , maxBsr
            , numberOfProductsToFind
            , numberOfPagesToSearch
            )
        {
            ProductSearcher = searcher;
            Log.Info("Starting navigator on thread " + Thread.CurrentThread.ManagedThreadId.ToString());
        }

        public override void PerformSearch()
        {
            List<Task> tasks = new List<Task>();

            // Create a new task for each search term, up to a maximum number of concurrent tasks
            int searchTermIndex = 0;
            const int maximumNumberOfConcurrentTasks = 2;
            while (true)
            {
                // Determine if we still have more search terms to look at
                if (searchTermIndex < SearchTerms.Count && tasks.Count < maximumNumberOfConcurrentTasks)
                {
                    // Add the next term to the list
                    tasks.Add(CreateAmazonNavigator(SearchTerms[searchTermIndex]));
                    searchTermIndex++;

                    Log.Info("Added task. " + tasks.Count + " out of " + maximumNumberOfConcurrentTasks + " active searches. Current search index: " + searchTermIndex);
                }

                // After 30 seconds, see what tasks finished
                Task.WaitAny(tasks.ToArray(), 2000);

                var results = tasks.FindAll(FindCompleteTasks);
                var finishedTasks = results.Count;

                foreach (var result in results)
                {
                    tasks.Remove(result);
                }

                // We are done
                if (tasks.Count == 0 && searchTermIndex == SearchTerms.Count)
                {
                    break;
                }
            }
        }

        private void DoSearchForProduct(string searchTerm, Searcher productSearcher, List<string> productCategories)
        {
            int attemptsForSearchTerm = 0;
            int maximumAttemptsToSearchForTerm = 3;

            for (int attempt = 0; attempt < maximumAttemptsToSearchForTerm; attempt++)
            {
                try
                {
                    Log.Info("Searching for " + searchTerm);

                    // Create our driver when the threadpool is ready to start processing us
                    var driver = DriverUtils.Make(searchTerm);

                    var recoveryUrl = driver.Url;
                    FindProductsForSearchTerm(driver, searchTerm, productSearcher, productCategories);

                    driver.Close();

                    break;
                }
                catch(Exception e) // Catch anything and keep trying
                {
                    Log.Error(searchTerm + ": Exception caught:" + e.Message);
                    // Something bad has happened during our search for this term.
                    if (++attemptsForSearchTerm == maximumAttemptsToSearchForTerm)
                    {
                        // We have failed to search for this term 3 times, move to the next term
                        Log.Error("Giving up on search term " + searchTerm + " as maximum retries exceeded");

                        attemptsForSearchTerm = 0;
                    }

                    Log.Error("Attempting to clean up old web driver for search term " + searchTerm);
                }
            }
        }

        private Task CreateAmazonNavigator(string searchTerm)
        {
            // Create the amazon navigator as an asynchronous task
            Task task = Task.Factory.StartNew(
                () =>
                {
                    DoSearchForProduct(searchTerm, SearcherFactory.Make(ProductSearcher), CategoriesToConsider);
                }
            , CancellationToken.None
            , TaskCreationOptions.LongRunning
            , TaskScheduler.Default);

            return task;
        }

        private static bool FindCompleteTasks(Task task)
        {
            return task.Status == TaskStatus.RanToCompletion;
        }

        private void FindProductsForSearchTerm(IWebDriver driver, string searchTerm, Searcher productSearcher, List<string> productCategories)
        {
            productSearcher.SearchForProduct(driver, searchTerm);

            if(!PageReader.NextPageButtonExists(driver))
            {
                Log.Info(searchTerm + " is being abandoned as only one page of results was returned");
                return;
            }

            int numberOfPagesSearched = 1;
            int numberOfProductsFound = 0;

            // Keep looking until we have found the requested number of products OR we have searched too many pages
            while (!StopSearchingProduct(driver, numberOfProductsFound, numberOfPagesSearched))
            {
                var productPageLink = driver.Url;
                var productLinks = PageReader.GetProductLinksOnPage(driver);
                Log.Info(searchTerm + ": found " + productLinks.Count + " products on page");

                foreach (var product in productLinks)
                {
                    DriverUtils.GoToLink(driver, product);
                    var rankings = PageReader.GetRankings(driver, searchTerm, productCategories, MaximumBSR);

                    if(rankings.ToList().Count > 0)
                    {
                        numberOfProductsFound++;

                        Log.Info(searchTerm + ": " + numberOfProductsFound + " products so far over " + (numberOfPagesSearched) + " pages");

                        Log.BsrRankings(rankings);

                        if (EnoughProductsHaveBeenFound(numberOfProductsFound))
                        {
                            // Don't continue processing products on this page if we have found enough
                            break;
                        }
                    }
                }

                // Move to the next page if we have not yet found enough products
                if (!EnoughProductsHaveBeenFound(numberOfProductsFound))
                {
                    // Go back to the product page after looking at all of the products
                    DriverUtils.GoToLink(driver, productPageLink);
                    MoveToNextPageOfSearchResults(driver);
                    numberOfPagesSearched++;
                }
            }

            PrintResults(searchTerm, numberOfProductsFound, numberOfPagesSearched);
        }
    }
}
