using System;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace SeleniumParser
{
    public class AmazonNavigator
    {
        // 400,000 selling once or twice a week
        // 100,000 selling one design a day
        // 2,000 25-50 a day
        static int MaximumBSR = 100000;
        static int NumberOfProductsToFind = 100;
        static int NumberOfPagesToGiveUpAfter = 100;

        static string SearchCategory = "Clothing, Shoes & Jewelry";

        static List<string> SearchTerms = new List<string> { 
            "cat t shirt"
            , "motivational t shirt"
            , "motivational exercise t shirt"
            , "saying t shirt"
            , "donald trump t shirt"
            , "anti donald trump t shirt"
            , "political t shirt"
            , "cat saying t shirt"
            , "dog saying t shirt"
            , "funny saying t shirt"
            , "french bulldog t shirt"
            , "pug t shirt"
            , "funny saying t shirt"
            , "coffee t shirt"
            , "tea t shirt"
            , "doughnut t shirt"
            , "salty t shirt"
            , "salty saying t shirt"
            , "snatched t shirt"
            , "werq t shirt"
            , "slay t shirt"
            , "yolo t shirt"
            , "a crapella t shirt"
            , "amazeballs t shirt"
            , "askhole t shirt"
            , "awesome sauce t shirt"
            , "baby bump t shirt"
            , "badassery t shirt"
            , "beer me t shirt"
            , "blamestorming t shirt"
            , "bootylicious t shirt"
            , "bromance t shirt"
            , "bye felicia t shirt"
            , "cougar t shirt"
            , "designated drunk t shirt"
            , "duck face t shirt"
            , "fanboi t shirt"
            , "fan girl t shirt"
            , "fauxpology t shirt"
            , "foodie t shirt"
            , "frak t shirt"
            , "friend zone t shirt"
            , "fro yo t shirt"
            , "fro-yo t shirt"
            , "gaydar t shirt"
            , "girl crush t shirt"
            , "grrrl t shirt"
            , "hangry t shirt"
            , "hipster t shirt"
            , "hot mess t shirt"
            , "humblebrag t shirt"
            , "jailbait t shirt"
            , "knosh t shirt"
            , "lol t shirt"
            , "make it rain t shirt"
            , "man boobs t shirt"
            , "man cave t shirt"
            , "man sweats t shirt"
            , "milf t shirt"
            , "netflix and chill t shirt"
            , "ninja sex t shirt"
            , "nom t shirt"
            , "nontroversy t shirt"
            , "nsfw t shirt"
            , "party foul t shirt"
            , "phat t shirt"
            , "pregret t shirt"
            , "pwned t shirt"
            , "quantum physics t shirt"
            , "ratchet t shirt"
            , "rendezbooz t shirt"
            , "rickroll t shirt"
            , "said no one ever t shirt"
            , "selfie t shirt"
            , "sext t shirt"
            , "side boob t shirt"
            , "that's what she said t shirt"
            , "trout t shirt"
            , "twerk t shirt"
            , "typeractive t shirt"
            , "wtf t shirt"
            , "yolo t shirt"
            , "zombie t shirt"
        };

        static List<string> CategoriesToConsider = new List<string> { 
            "T-Shirts"
            , "Tank Tops"
            , "T-Shirts & Tanks"
            , "Tanks & Camis" 
        };
        
        static void Main(string[] args)
        {
            // Identify the current running chrome processes, so that we do not kill them if we fail during product search
            List<int> chromePids = GetRunningChromeProcessIds();

            // Create our driver after getting the running chrome instances
            var driver = Driver.Make();

            int searchTermIndex = 0;
            bool continueLooping = true;
            int attemptsForSearchTerm = 0;
            int maximumAttemptsToSearchForTerm = 3;
            
            while (continueLooping)
            {
                try
                {
                    var recoveryUrl = driver.Url;
                    FindProductsForSearchTerm(driver, SearchTerms[searchTermIndex]);

                    // The search term has passed, set the attempts back to zero
                    attemptsForSearchTerm = 0;

                    // Add 1 to zero based index before seeing if we are at the end of the list
                    if(searchTermIndex + 1 < SearchTerms.Count)
                    {
                        searchTermIndex++;
                    }
                    else
                    {
                        continueLooping = false;
                    }
                }
                catch // Catch anything and keep trying
                {
                    // Something bad has happened during our search for this term.
                    if (++attemptsForSearchTerm == maximumAttemptsToSearchForTerm)
                    {
                        // We have failed to search for this term 3 times, move to the next term
                        Log.Error("Giving up on search term " + SearchTerms[searchTermIndex] + " as maximum retries exceeded");

                        searchTermIndex++;
                        attemptsForSearchTerm = 0;
                    }

                    Log.Error("Attempting to clean up old web driver");
                    TryToCleanUpFromException(chromePids, driver);

                    // Now that we have tried to recover by cleaning up, create a new web driver
                    driver = Driver.Make();
                    
                    // Frin this point on we will retry the last search term with an incremented attempt count (so that we do not loop forever on a crap search term)
                }
            }

            Console.ReadKey();

            // Close the browser
            driver.Close();
        }

        /// <summary>
        /// Moves to the next page of search results
        /// </summary>
        /// <param name="driver"></param>
        private static void MoveToNextPageOfSearchResults(IWebDriver driver)
        {
            driver.FindElement(By.Id("pagnNextString")).Click();

            Driver.WaitForPageToLoad();
        }

        /// <summary>
        /// Kill chrome processes started be selenium and try and close the driver
        /// </summary>
        /// <param name="chromePids"></param>
        /// <param name="driver"></param>
        private static void TryToCleanUpFromException(List<int> chromePids, IWebDriver driver)
        {
            try
            {
                // Try to close the current driver down
                KillChromeProcesses(chromePids);

                // Trying to close the chrome driver when the window has been closed will likely throw out some sort of selenium exception
                driver.Close();
            }
            catch { } // Ignore failures during cleanup - try and move on from this disaster!
        }

        /// <summary>
        /// Kills all chrome processes except those specified in parameters
        /// </summary>
        /// <param name="chromePids"></param>
        private static void KillChromeProcesses(List<int> chromePids)
        {
            // Kill any running instances of google chrome
            foreach (var process in Process.GetProcessesByName("Chrome"))
            {
                // Only kill the process if it was not running before we created the driver
                if (!chromePids.Contains(process.Id))
                {
                    process.Kill();
                }
            }
        }

        /// <summary>
        /// Gets process ids of all currently running chrome instances and extensions
        /// </summary>
        /// <returns></returns>
        private static List<int> GetRunningChromeProcessIds()
        {
            List<int> chromePids = new List<int>();

            // Kill any running instances of google chrome
            foreach (var process in Process.GetProcessesByName("Chrome"))
            {
                chromePids.Add(process.Id);
            }

            return chromePids;
        }

        /// <summary>
        /// Iterates through search results until:
        /// 1. The configured number of products has been found
        /// 2. The configured number of search pages has been exceeded
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="searchTerm"></param>
        private static void FindProductsForSearchTerm(IWebDriver driver, string searchTerm)
        {
            // Look up the term using the search bar
            SearchUsingAmazonSearchBar(driver, searchTerm);

            Driver.WaitForPageToLoad();

            int numberOfPagesSearched = 0;
            int numberOfProductsFound = 0;

            // Keep looking until we have found the requested number of products OR we have searched too many pages
            while (!StopSearchingProduct(numberOfPagesSearched, numberOfProductsFound))
            {
                var productPageLink = driver.Url;
                var productLinks = GetProductLinksOnPage(driver);

                foreach (var product in productLinks)
                {
                    var rankings = FindRankingsForProduct(driver, product);
                    Log.BsrRankings(searchTerm, rankings);

                    // Don't continue processing products on this page if we have found enough
                    if (EnoughProductsHaveBeenFound(++numberOfProductsFound))
                    {
                        break;
                    }
                }

                // Go back to the product page after looking at all of the products
                Driver.GoToLink(driver, productPageLink);
                MoveToNextPageOfSearchResults(driver);

                numberOfPagesSearched++;
            }
        }

        /// <summary>
        /// Determines whether or not we have exceeded the thresholds for number of products found or number of pages searched
        /// </summary>
        /// <param name="numberOfPagesSearched"></param>
        /// <param name="numberOfProductsFound"></param>
        /// <returns></returns>
        private static bool StopSearchingProduct(int numberOfPagesSearched, int numberOfProductsFound)
        {
            return  EnoughProductsHaveBeenFound(numberOfProductsFound) ||
                    EnoughPagesHaveBeenSearched(numberOfPagesSearched);
        }

        /// <summary>
        /// Returns whether enough pages have been searched based off configured value
        /// </summary>
        /// <param name="numberOfPagesSearched"></param>
        /// <returns></returns>
        private static bool EnoughPagesHaveBeenSearched(int numberOfPagesSearched)
        {
            return numberOfPagesSearched < NumberOfPagesToGiveUpAfter;
        }

        /// <summary>
        /// Returns whether enough products have been searched based off configured value
        /// </summary>
        /// <param name="numberFound"></param>
        /// <returns></returns>
        private static bool EnoughProductsHaveBeenFound(int numberFound)
        {
            return numberFound > NumberOfProductsToFind;
        }

        /// <summary>
        /// Go to the product page and find its bsr rankings
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="productLink"></param>
        /// <returns></returns>
        private static IEnumerable<BsrRank> FindRankingsForProduct(IWebDriver driver, string productLink)
        {
            Driver.GoToLink(driver, productLink);

            return GetRankings(driver);
        }

        /// <summary>
        /// Selects the configured category from the dropdown list next to the search bar
        /// </summary>
        private static void SelectProductCategoryForSearchBar(IWebDriver driver)
        {
            // Ensure that we are searching with the right category. Each category apparently has different page formatting
            var categoryDropdown = driver.FindElement(By.ClassName("nav-search-dropdown"));
            var selectElementDropdown = new SelectElement(categoryDropdown);

            // Select the required category
            selectElementDropdown.SelectByText(SearchCategory);
        }

        /// <summary>
        /// Chooses product category, and then searches for the specified term by typing into search box and clicking the search button
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="searchTerm"></param>
        public static void SearchUsingAmazonSearchBar(IWebDriver driver, string searchTerm)
        {
            SelectProductCategoryForSearchBar(driver);

            // Get the search box
            IWebElement searchBox = driver.FindElement(By.Id("twotabsearchtextbox"));

            searchBox.Clear();

            // Search for the term
            searchBox.SendKeys(searchTerm);

            // Click the search button
            var searchButton = driver.FindElement(By.ClassName("nav-input"));
            searchButton.Click();
        }

        /// <summary>
        /// Get products on the search results page
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        public static List<string> GetProductLinksOnPage(IWebDriver driver)
        {
            Console.WriteLine("Getting product links");

            // Get the results container
            var resultsContainer = driver.FindElement(By.Id("atfResults"));

            // Find the objects that have the a-row and a-spacing-mini class names
            var productResults = resultsContainer.FindElements(By.CssSelector(".a-row.a-spacing-micro"));

            List<string> strings = new List<string>();
            foreach (var productResult in productResults)
            {
                var text = productResult.FindElement(By.CssSelector("a"));
                var productLink = text.GetAttribute("href");

                strings.Add(productLink);
            }

            Console.WriteLine("Found " + strings.Count + " links");

            return strings;
        }

        /// <summary>
        /// Collects all bsr categories that are within the configured bsr rank threshold
        /// Returns an empty collection of results when no categories matched
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        static IEnumerable<BsrRank> GetRankings(IWebDriver driver)
        {
            List<BsrRank> bsrRanks = new List<BsrRank>();

            try
            {
                // Find the sales rank section on the page - the topmost parent of the list of ranks that apply to the product
                var salesRank = driver.FindElement(By.ClassName("zg_hrsr"));

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
                            var productTitle = driver.FindElement(By.Id("title")).Text;

                            // Add the ranking to the collection that we want to look at
                            bsrRanks.Add(new BsrRank ( 
                                driver.Url 
                                , productTitle
                                , categoryName
                                , rank
                                , rankingString
                            ));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // If the product is really crap, it may not rank on anything
                // Catch exception thrown in this case and just move on...
                Log.Error("Exception caught trying to consider " + driver.Url + ". Exception: " + e.Message);
            }

            return bsrRanks;
        }
    }
}
