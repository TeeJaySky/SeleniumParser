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
            var driver = Utils.CreateDriver();

            int searchTermIndex = 0;
            bool continueLooping = true;
            int attemptsForSearchTerm = 0;
            
            while (continueLooping)
            {
                try
                {
                    var recoveryUrl = driver.Url;
                    FindProductsForTerm(driver, SearchTerms[searchTermIndex]);

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
                    if (++attemptsForSearchTerm == 3)
                    {
                        // We have failed to search for this term 3 times, move to the next term
                        Utils.WriteError("Giving up on search term " + SearchTerms[searchTermIndex] + " as maximum retries exceeded");

                        searchTermIndex++;
                        attemptsForSearchTerm = 0;
                    }

                    try
                    {
                        Utils.WriteError("Re-instantiating driver due to exception");
                        driver = RecoverWebDriver(chromePids, driver);
                    }
                    catch { } // Catch anything that might happen and try again move on

                    // We will retry the last search term that the failure occurred from from this point onwards
                }
            }

            Console.ReadKey();

            // Close the browser
            driver.Close();
        }

        private static IWebDriver RecoverWebDriver(List<int> chromePids, IWebDriver driver)
        {
            // Try to close the current driver down
            driver.Close();

            KillChromeProcesses(chromePids);

            // Start again...
            driver = Utils.CreateDriver();
            return driver;
        }

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

        private static void FindProductsForTerm(IWebDriver driver, string term)
        {
            // Look up the term using the search bar
            SearchUsingAmazonSearchBar(driver, term);

            Utils.WaitForPageToLoad();

            int numberOfPagesSearched = 0;
            int numberOfProductsFound = 0;
            // Keep looking until we have found the requested number of products OR we have searched too many pages
            while (numberOfProductsFound < NumberOfProductsToFind && numberOfPagesSearched < NumberOfPagesToGiveUpAfter)
            {
                numberOfProductsFound += FindProductsWorthConsideringOnCurrentPage(driver, term, numberOfProductsFound);

                Utils.MoveToNextPage(driver);
                numberOfPagesSearched++;
            }
        }

        public static void SearchUsingAmazonSearchBar(IWebDriver driver, string searchTerm)
        {
            // Ensure that we are searching with the right category. Each category apparently has different page formatting
            var categoryDropdown = driver.FindElement(By.ClassName("nav-search-dropdown"));
            var selectElementDropdown = new SelectElement(categoryDropdown);

            // Select the required category
            selectElementDropdown.SelectByText(SearchCategory);

            // Get the search box
            IWebElement searchBox = driver.FindElement(By.Id("twotabsearchtextbox"));

            searchBox.Clear();

            // Search for the term
            searchBox.SendKeys(searchTerm);

            // Click the search button
            var searchButton = driver.FindElement(By.ClassName("nav-input"));
            searchButton.Click();
        }

        private static int FindProductsWorthConsideringOnCurrentPage(IWebDriver driver, string term, int currentProductsFound)
        {
            int numberFound = 0;
            // Take a copy of the current page, so that we can return to this after evaluating each product
            var productPageLink = driver.Url;

            foreach (var productLink in GetProductLinksOnPage(driver))
            {
                // Add some space on console before outputting where we are going
                Utils.WriteBlankLines(2);
                Utils.GoToLink(driver, productLink);

                try
                {
                    var consideration = GetDesignConsideration(driver);

                    if(consideration.BestSellerCategoryToRank.Count > 0)
                    {
                        numberFound++;
                        Utils.WriteToFile(term, consideration);
                        
                        // Stop as soon as we have found the required number of products
                        if (numberFound + currentProductsFound > NumberOfProductsToFind)
                        {
                            break;
                        }
                    }
                }
                catch(Exception e)
                {
                    // Catch and keep on powering on!
                    Utils.WriteError("Exception caught trying to consider " + productLink + ". Exception: " + e.Message);
                }
            }

            // Go back to the product page after looking at all of the products
            Utils.GoToLink(driver, productPageLink);

            return numberFound;
        }

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

        static SuccessDescriptor GetDesignConsideration(IWebDriver driver)
        {
            // Find the sales rank section on the page - the topmost parent of the list of ranks that apply to the product
            var salesRank = driver.FindElement(By.ClassName("zg_hrsr"));

            SuccessDescriptor successDescription = new SuccessDescriptor(driver.Url);

            // Find the list of items in this container
            var ranks = salesRank.FindElements(By.CssSelector("li"));

            foreach (var rank in ranks)
            {
                // The text is in a b tag
                var categoryName = rank.FindElement(By.CssSelector("b")).Text;

                // Only consdier if the rank is in one of the specified categories
                if (CategoriesToConsider.Contains(categoryName))
                {
                    categoryName = Utils.ProcessCategoryName(rank, categoryName);

                    // Trim off the hash from the rank
                    var rankingString = rank.FindElement(By.ClassName("zg_hrsr_rank")).Text.Substring(1);
                    var rankingInt = Convert.ToInt32(rankingString);

                    if( rankingInt <= MaximumBSR)
                    {
                        // Store the title for this product now that we want to consider it (delayed DOM query to increase efficiency)
                        var productTitle = driver.FindElement(By.Id("title")).Text;
                        Console.WriteLine(productTitle + " considered with ranking " + rankingString + " for category " + categoryName);
                        successDescription.Title = productTitle;

                        // Store the category and its associated rank. A product may be successful across different categories!
                        successDescription.BestSellerCategoryToRank.Add(categoryName, rankingInt);
                    }
                }
            }

            return successDescription;
        }
    }
}
