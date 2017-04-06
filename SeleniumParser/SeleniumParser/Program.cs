using System;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumParser
{
    public class AmazonNavigator
    {
        // 400,000 selling once or twice a week
        // 100,000 selling one design a day
        // 2,000 25-50 a day
        static int MaximumBSR = 2000;
        static int NumberOfProductsToFind = 20;
        static int NumberOfPagesToGiveUpAfter = 100;

        static string SearchCategory = "Clothing, Shoes & Jewelry";

        static List<string> SearchTerms = new List<string> { "cat t shirt" }; //"cat saying t shirt", "dog saying t shirt", "funny saying t shirt"};
        static List<string> CategoriesToConsider = new List<string> { "T-Shirts", "Tank Tops", "T-Shirts & Tanks", "Tanks & Camis" };
        
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver(@"C:\Users\Trent\Desktop\Projects\SeleniumParser\SeleniumParser");
            driver.Url = "http://www.amazon.com";
            Utils.WaitForPageToLoad();

            List<string> productPagesToReview = new List<string>();

            foreach (var term in SearchTerms)
            {
                // Look up the term using the search bar
                SearchUsingAmazonSearchBar(driver, term);

                Utils.WaitForPageToLoad();

                int numberOfPagesSearched = 0;
                // Keep looking until we have found the requested number of products OR we have searched too many pages
                while (productPagesToReview.Count < NumberOfProductsToFind && numberOfPagesSearched < NumberOfPagesToGiveUpAfter)
                {
                    FindProductsWorthConsideringOnCurrentPage(driver, term);

                    Utils.MoveToNextPage(driver);
                    numberOfPagesSearched++;
                }

                //Utils.WriteLinksToFileForSearchTerm(term, productPagesToReview);
            }

            Console.ReadKey();

            // Close the browser
            driver.Close();
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

        private static void FindProductsWorthConsideringOnCurrentPage(IWebDriver driver, string term)
        {
            // Take a copy of the current page, so that we can return to this after evaluating each product
            var productPageLink = driver.Url;

            foreach (var productLink in GetProductLinksOnPage(driver))
            {
                // Add some space on console before outputting where we are going
                Utils.WriteBlankLines(2);
                Utils.GoToLink(driver, productLink);

                var consideration = GetDesignConsideration(driver);
                Utils.WriteToFile(term, consideration);
            }

            // Go back to the product page after looking at all of the products
            Utils.GoToLink(driver, productPageLink);
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
                        Console.WriteLine("Considered with ranking " + rankingString + " for category " + categoryName);

                        // Store the category and its associated rank. A product may be successful across different categories!
                        successDescription.BestSellerCategoryToRank.Add(categoryName, rankingInt);
                    }
                }
            }

            return successDescription;
        }
    }
}
