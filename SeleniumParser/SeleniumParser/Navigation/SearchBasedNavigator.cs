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
        #region Old Search Terms
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
           , "gin"
           , "wine"
           , "drinking"
           , "fishing"
           , "golf"
           , "wrestling"
           , "football"
           , "soccer"
        };
        #endregion

        #region Old Novelty Womens Tees Search
        //static List<string> SearchTerms = new List<string> { 
        //    "Cat"
        //    , "funny"
        //    , "sarcastic"
        //    , "dog"
        //    , "Coffee"
        //    , "Exercise"
        //    , "Gym"
        //    , "doughnut"
        //    , "wine"
        //    , "gin"
        //    , "vodka"
        //    , "drinking"
        //    , "bride"
        //    , "bridesmaid"
        //    , "sister"
        //    , "salty"
        //    , "Motivation"
        //    , "yolo"
        //    , "lazy"
        //    , "fangirl"
        //    , "cougar"
        //    , "hot mess"
        //    , "wife"
        //    , "mother"
        //    , "daughter"
        //    , "cooking"
        //    , "baking"
        //    , "tea"
        //    , "pink"
        //    , "girl"
        //    , "woman"
        //};
        #endregion

        int MaximumNumberOfConcurrentTasks;
        int NumberOfPagesToSearchWithoutProducts;
        SearcherFactory.SearcherType ProductSearcher;

        public SearchBasedNavigator(
            SearcherFactory.SearcherType searcher
            , List<string> categoriesToConsider
            , int maxBsr = 100000
            , int numberOfProductsToFind = 50
            , int numberOfPagesToSearch = 30
            , int numberOfPagesToSearchWithoutProducts = 3
            , int maximumNumberOfConcurrentTasks = 10
            )
            : base(
            categoriesToConsider
            , maxBsr
            , numberOfProductsToFind
            , numberOfPagesToSearch
            )
        {
            ProductSearcher = searcher;
            MaximumNumberOfConcurrentTasks = maximumNumberOfConcurrentTasks;
            NumberOfPagesToSearchWithoutProducts = numberOfPagesToSearchWithoutProducts;

            Log.Info("Starting navigator on thread " + Thread.CurrentThread.ManagedThreadId.ToString());
        }

        public override void PerformSearch()
        {
            List<Task> tasks = new List<Task>();

            // Create a new task for each search term, up to a maximum number of concurrent tasks
            int searchTermIndex = 0;
            while (true)
            {
                // Determine if we still have more search terms to look at
                if (searchTermIndex < SearchTerms.Count && tasks.Count < MaximumNumberOfConcurrentTasks)
                {
                    // Add the next term to the list
                    tasks.Add(CreateAmazonNavigator(SearchTerms[searchTermIndex]));
                    searchTermIndex++;

                    Log.Info("Added task. " + tasks.Count + " out of " + MaximumNumberOfConcurrentTasks + " active searches. Current search index: " + searchTermIndex);
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
            int numberOfProductsFoundOnPage = 0;
            int numberOfPagesSearchedWithoutProduct = 0;

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
                        numberOfPagesSearchedWithoutProduct = 0;
                        numberOfProductsFoundOnPage++;
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

                if (numberOfProductsFoundOnPage == 0)
                {
                    Log.Info(searchTerm + ": found zero products on page " + numberOfPagesSearched);
                    numberOfPagesSearchedWithoutProduct++;

                    if (numberOfPagesSearchedWithoutProduct == NumberOfPagesToSearchWithoutProducts)
                    {
                        Log.Info(searchTerm + ": maximum number of pages searched without finding product. Giving up");
                        break;
                    }
                }

                numberOfProductsFoundOnPage = 0;

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
