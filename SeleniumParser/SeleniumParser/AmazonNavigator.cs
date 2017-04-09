using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class AmazonNavigator
    {
        // 400,000 selling once or twice a week
        // 100,000 selling one design a day
        // 2,000 25-50 a day
        int MaximumBSR = 100000;
        int NumberOfProductsToFind = 100;
        int NumberOfPagesToGiveUpAfter = 100;

        int numberOfPagesSearched = 0;
        int numberOfProductsFound = 0;

        string SearchTerm;
        IWebDriver Driver;
        List<string> CategoriesToConsider;
        string SearchCategory;

        public AmazonNavigator(string searchTerm ,List<string> categoriesToConsider, string searchCategory)
        {
            SearchTerm = searchTerm;
            CategoriesToConsider = categoriesToConsider;
            SearchCategory = searchCategory;
        }

        public override string ToString()
        {
            return SearchTerm + ": " + numberOfProductsFound + " products found over " + numberOfPagesSearched + " pages";
        }

        public void PerformSearch()
        {
            // Create our driver when the threadpool is ready to start processing us
            Driver = DriverUtils.Make();

            int attemptsForSearchTerm = 0;
            int maximumAttemptsToSearchForTerm = 3;

            for (int attempt = 0; attempt < maximumAttemptsToSearchForTerm; attempt++)
            {
                try
                {
                    var recoveryUrl = Driver.Url;
                    FindProductsForSearchTerm();
                }
                catch // Catch anything and keep trying
                {
                    // Something bad has happened during our search for this term.
                    if (++attemptsForSearchTerm == maximumAttemptsToSearchForTerm)
                    {
                        // We have failed to search for this term 3 times, move to the next term
                        Log.Error("Giving up on search term " + SearchTerm + " as maximum retries exceeded");

                        attemptsForSearchTerm = 0;
                    }

                    Log.Error("Attempting to clean up old web driver");
                    TryToCleanUpFromException();

                    // Now that we have tried to recover by cleaning up, create a new web driver
                    Driver = DriverUtils.Make();
                }
            }

            // Close the browser
            Driver.Close();
        }

        /// <summary>
        /// Moves to the next page of search results
        /// </summary>
        private void MoveToNextPageOfSearchResults()
        {
            Driver.FindElement(By.Id("pagnNextString")).Click();

            DriverUtils.WaitForPageToLoad();
        }

        /// <summary>
        /// Kill chrome processes started be selenium and try and close the driver
        /// </summary>
        private void TryToCleanUpFromException()
        {
            try
            {
                // Trying to close the chrome driver when the window has been closed will likely throw out some sort of selenium exception
                Driver.Close();
            }
            catch { } // Ignore failures during cleanup - try and move on from this disaster!
        }

        /// <summary>
        /// Iterates through search results until:
        /// 1. The configured number of products has been found
        /// 2. The configured number of search pages has been exceeded
        /// </summary>
        private void FindProductsForSearchTerm()
        {
            // Look up the term using the search bar
            SearchUsingAmazonSearchBar();

            // Keep looking until we have found the requested number of products OR we have searched too many pages
            while (!StopSearchingProduct(numberOfPagesSearched, numberOfProductsFound))
            {
                var productPageLink = Driver.Url;
                var productLinks = GetProductLinksOnPage();

                if (productLinks.Count == 0)
                {
                    throw new Exception("No product links found. Move onto next product");
                }

                foreach (var product in productLinks)
                {
                    var rankings = FindRankingsForProduct(product);
                    Log.BsrRankings(rankings);

                    if (EnoughProductsHaveBeenFound(++numberOfProductsFound))
                    {
                        // Don't continue processing products on this page if we have found enough
                        break;
                    }
                }

                // Move to the next page if we have not yet found enough products
                if (!EnoughProductsHaveBeenFound(numberOfProductsFound))
                {
                    // Go back to the product page after looking at all of the products
                    DriverUtils.GoToLink(Driver, productPageLink);
                    MoveToNextPageOfSearchResults();
                    numberOfPagesSearched++;
                }
            }
        }

        /// <summary>
        /// Determines whether or not we have exceeded the thresholds for number of products found or number of pages searched
        /// </summary>
        /// <param name="numberOfPagesSearched"></param>
        /// <param name="numberOfProductsFound"></param>
        /// <returns></returns>
        private  bool StopSearchingProduct(int numberOfPagesSearched, int numberOfProductsFound)
        {
            return EnoughProductsHaveBeenFound(numberOfProductsFound) ||
                    EnoughPagesHaveBeenSearched(numberOfPagesSearched);
        }

        /// <summary>
        /// Returns whether enough pages have been searched based off configured value
        /// </summary>
        /// <param name="numberOfPagesSearched"></param>
        /// <returns></returns>
        private  bool EnoughPagesHaveBeenSearched(int numberOfPagesSearched)
        {
            return numberOfPagesSearched > NumberOfPagesToGiveUpAfter;
        }

        /// <summary>
        /// Returns whether enough products have been searched based off configured value
        /// </summary>
        /// <param name="numberFound"></param>
        /// <returns></returns>
        private bool EnoughProductsHaveBeenFound(int numberFound)
        {
            return numberFound > NumberOfProductsToFind;
        }

        /// <summary>
        /// Go to the product page and find its bsr rankings
        /// </summary>
        /// <param name="productLink"></param>
        private IEnumerable<BsrRank> FindRankingsForProduct(string productLink)
        {
            DriverUtils.GoToLink(Driver, productLink);

            return GetRankings();
        }

        /// <summary>
        /// Selects the configured category from the dropdown list next to the search bar
        /// </summary>
        private void SelectProductCategoryForSearchBar()
        {
            // Ensure that we are searching with the right category. Each category apparently has different page formatting
            var categoryDropdown = Driver.FindElement(By.ClassName("nav-search-dropdown"));
            var selectElementDropdown = new SelectElement(categoryDropdown);

            // Select the required category
            selectElementDropdown.SelectByText(SearchCategory);
        }

        /// <summary>
        /// Chooses product category, and then searches for the specified term by typing into search box and clicking the search button
        /// </summary>
        public void SearchUsingAmazonSearchBar()
        {
            SelectProductCategoryForSearchBar();

            // Get the search box
            IWebElement searchBox = Driver.FindElement(By.Id("twotabsearchtextbox"));

            searchBox.Clear();

            // Search for the term
            searchBox.SendKeys(SearchTerm);

            // Click the search button
            var searchButton = Driver.FindElement(By.ClassName("nav-input"));
            searchButton.Click();

            DriverUtils.WaitForPageToLoad();
        }

        /// <summary>
        /// Get products on the search results page
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public List<string> GetProductLinksOnPage()
        {
            Log.Info("Getting product links");

            // Get the results container
            var resultsContainer = Driver.FindElement(By.Id("atfResults"));

            // Find the objects that have the a-row and a-spacing-mini class names
            var productResults = resultsContainer.FindElements(By.CssSelector(".a-row.a-spacing-micro"));

            List<string> strings = new List<string>();
            foreach (var productResult in productResults)
            {
                var text = productResult.FindElement(By.CssSelector("a"));
                var productLink = text.GetAttribute("href");

                strings.Add(productLink);
            }

            Log.Info("Found " + strings.Count + " links");

            return strings;
        }

        /// <summary>
        /// Collects all bsr categories that are within the configured bsr rank threshold
        /// Returns an empty collection of results when no categories matched
        /// </summary>
        IEnumerable<BsrRank> GetRankings()
        {
            List<BsrRank> bsrRanks = new List<BsrRank>();

            try
            {
                // Find the sales rank section on the page - the topmost parent of the list of ranks that apply to the product
                var salesRank = Driver.FindElement(By.ClassName("zg_hrsr"));

                // Find the list of items in this container
                var ranks = salesRank.FindElements(By.CssSelector("li"));

                foreach (var rank in ranks)
                {
                    // The text is in a b tag
                    var categoryName = rank.FindElement(By.CssSelector("b")).Text;

                    // Only consdier if the rank is in one of the specified categories
                    if (CategoriesToConsider.Contains(categoryName))
                    {
                        // Trim off the hash from the rank
                        var rankingString = rank.FindElement(By.ClassName("zg_hrsr_rank")).Text.Substring(1);
                        var rankingInt = Convert.ToInt32(rankingString);

                        if (rankingInt <= MaximumBSR)
                        {
                            // Store the title for this product now that we want to consider it (delayed DOM query to increase efficiency)
                            var productTitle = Driver.FindElement(By.Id("title")).Text;

                            // Add the ranking to the collection that we want to look at
                            bsrRanks.Add(new BsrRank(
                                Driver.Url
                                , productTitle
                                , categoryName
                                , rank
                                , rankingString
                                , SearchTerm
                            ));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // If the product is really crap, it may not rank on anything
                // Catch exception thrown in this case and just move on...
                //Log.Error("Exception caught trying to consider " + Driver.Url + ". Exception: " + e.Message);
            }

            return bsrRanks;
        }
    }
}
