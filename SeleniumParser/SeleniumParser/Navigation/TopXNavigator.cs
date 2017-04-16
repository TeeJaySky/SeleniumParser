using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class TopXNavigator : Navigator
    {
        string SearchTerm = " ";
        Searcher ProductSearcher;
        int numberOfProductsFound = 0;

        public TopXNavigator(
            Searcher searcher
            , List<string> categoriesToConsider
            , int maxBsr = 50000
            , int numberOfProductsToFind = 2500
            , int numberOfPagesToSearch = 50000
            )
 
            : base(
            categoriesToConsider
            , maxBsr
            , numberOfProductsToFind
            , numberOfPagesToSearch
            )
        {
            ProductSearcher = searcher;
        }

        public override void PerformSearch()
        {
            var driver = DriverUtils.Make(SearchTerm);
            ProductSearcher.SearchForProduct(driver, SearchTerm);

            int currentPageNumber = 1;
            int maximumNumberOfConcurrentTasks = 15;

            // For each page in the results, create a new task
            List<Task> tasks = new List<Task>();

            // Keep looking until we have found the requested number of products OR we have searched too many pages
            while (!StopSearchingProduct(driver, numberOfProductsFound, currentPageNumber))
            {
                if (tasks.Count < maximumNumberOfConcurrentTasks)
                {
                    // Add the next term to the list
                    tasks.Add(CreateTask(driver, currentPageNumber));

                    MoveToNextPageOfSearchResults(driver);

                    Interlocked.Increment(ref currentPageNumber);

                    Log.Info("Added task. " + tasks.Count + " out of " + maximumNumberOfConcurrentTasks + " active page searches. Next page to search is: " + currentPageNumber);
                }

                // After 30 seconds, see what tasks finished
                Task.WaitAny(tasks.ToArray(), 10000);

                var results = tasks.FindAll(FindCompleteTasks);
                var finishedTasks = results.Count;

                foreach (var result in results)
                {
                    tasks.Remove(result);
                }
            }

            // Let tasks finish if we still havent found enough products
            bool productsFound = false;
            while(!productsFound)
            {
                if(EnoughProductsHaveBeenFound(numberOfProductsFound))
                {
                    break;
                }

                Task.WaitAll(tasks.ToArray(), 2000);
            }

            PrintResults(SearchTerm, numberOfProductsFound, currentPageNumber);

            driver.Close();
            // Done
        }

        public Task CreateTask(IWebDriver driver, int pageNumber)
        {
            // Search for products on the page in a new thread
            Task task = Task.Factory.StartNew(
                () =>
                {
                    SearchForProductsOnPage(String.Copy(driver.Url), pageNumber);
                }
                , CancellationToken.None
                , TaskCreationOptions.LongRunning
                , TaskScheduler.Default
            );

            return task;
        }

        private void SearchForProductsOnPage(string url, int pageNumber)
        {
            // Make a new driver to direct it to the search results page
            var driver = DriverUtils.Make(SearchTerm);
            DriverUtils.GoToLink(driver, url);

            List<string> productLinks;

            if(pageNumber == 1)
            {
                productLinks = PageReader.GetProductLinksOnPageEmptySearchTerm(driver);
            }
            else
            {
                productLinks = PageReader.GetProductLinksOnPage(driver);
            }
            Log.Info(SearchTerm + ": found " + productLinks.Count + " products on page");

            foreach (var product in productLinks)
            {
                //DriverUtils.GoToLink(driver, product);
                driver.Url = product;

                var rankings = PageReader.GetRankings(driver, SearchTerm, CategoriesToConsider, MaximumBSR);

                if (rankings.ToList().Count > 0)
                {
                    Interlocked.Increment(ref numberOfProductsFound);

                    Log.Info(SearchTerm + ": " + numberOfProductsFound + " products so far over ");

                    Log.BsrRankings(rankings);

                    if (EnoughProductsHaveBeenFound(numberOfProductsFound))
                    {
                        // Don't continue processing products on this page if we have found enough
                        break;
                    }
                }
            }
        }

        private bool FindCompleteTasks(Task task)
        {
            return task.Status == TaskStatus.RanToCompletion;
        }
    }
}
