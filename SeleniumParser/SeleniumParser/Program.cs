using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;

namespace SeleniumParser
{
    public class AmazonNavigator
    {
        static int NumberOfProductsToFind = 2;
        static List<string> SearchTerms = new List<string>{"cat t shirt"};
        static List<string> CategoriesToConsider = new List<string> { "T-Shirts", "T-Shirts & Tanks" };
        // 400,000 selling once or twice a week
        // 100,000 selling one design a day
        // 2,000 25-50 a day
        static int MaximmRank = 2000;
        static string OutputFileDir = "Links.txt";
        static int PageLoadDuration = 1500;

        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver(@"C:\Users\Trent\Desktop\Projects\SeleniumParser\SeleniumParser");
            driver.Url = "http://www.amazon.com";
            Thread.Sleep(PageLoadDuration);

            List<string> productPagesToReview = new List<string>();

            foreach (var term in SearchTerms)
            {
                // Look up the term using the search bar
                SearchUsingAmazonSearchBar(driver, term);

                WaitForPageToLoad();

                // Keep looking until we have found the requested number of products
                while (productPagesToReview.Count < NumberOfProductsToFind)
                {
                    // Add any good products from the current page to the collection
                    productPagesToReview.AddRange(GetProductsWorthConsideringOnCurrentPage(driver));

                    MoveToNextPage(driver);
                }

                WriteLinksToFileForSearchTerm(term, productPagesToReview);
            }

            Console.ReadKey();

            // Close the browser
            driver.Close();
        }

        private static List<string> GetProductsWorthConsideringOnCurrentPage(IWebDriver driver)
        {
            // Take a copy of the current page, so that we can return to this after evaluating each product
            var productPageLink = driver.Url;
            List<string> productsToConsider = new List<string>();

            foreach (var productLink in GetProductLinksOnPage(driver))
            {
                GoToLink(driver, productLink);

                if (DesignShouldBeConsidered(driver))
                {
                    // Take a copy of the curren URL
                    productsToConsider.Add(driver.Url);
                }
            }

            // Go back to the product page after looking at all of the products
            GoToLink(driver, productPageLink);

            return productsToConsider;
        }

        static void GoToLink(IWebDriver driver, string url)
        {
            Console.WriteLine("Navigating to: " + url);
            driver.Url = url;

            WaitForPageToLoad();
        }

        static bool DesignShouldBeConsidered(IWebDriver driver)
        {
            // Find the sales rank section on the page - the topmost parent of the list of ranks that apply to the product
            var salesRank = driver.FindElement(By.ClassName("zg_hrsr"));

            // Find the list of items in this container
            var ranks = salesRank.FindElements(By.CssSelector("li"));
            foreach (var rank in ranks)
            {
                // The text is in a b tag
                var text = rank.FindElement(By.CssSelector("b")).Text;

                Console.WriteLine("Category: " + text);

                // Only consdier if the rank is in one of the specified categories
                if (CategoriesToConsider.Contains(text))
                {
                    // Trim off the hash from the rank
                    var rankingString = rank.FindElement(By.ClassName("zg_hrsr_rank")).Text.Substring(1);
                    var rankingInt = Convert.ToInt32(rankingString);

                    Console.WriteLine("Ranking is: " + rankingString);

                    if( rankingInt <= MaximmRank)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        
        private static void WaitForPageToLoad()
        {
            // Wait for page to load
            Thread.Sleep(PageLoadDuration);
        }

        private static void WriteLinksToFileForSearchTerm(string term, List<string> productLinks)
        {
            using(var writer = File.AppendText(OutputFileDir))
            {
                writer.WriteLine("Links for " + term);
                foreach(var link in productLinks)
                { 
                    writer.WriteLine(link);
                }
            }
        }

        public static void MoveToNextPage(IWebDriver driver)
        {
            driver.FindElement(By.Id("pagnNextString")).Click();
            
            WaitForPageToLoad();
        }

        public static void SearchUsingAmazonSearchBar(IWebDriver driver, string searchTerm)
        {
            // Get the search box
            IWebElement searchBox = driver.FindElement(By.Id("twotabsearchtextbox"));

            searchBox.Clear();

            // Search for the term
            searchBox.SendKeys(searchTerm);

            // Click the search button
            var searchButton = driver.FindElement(By.ClassName("nav-input"));
            searchButton.Click();
        }

        public static List<string> GetProductLinksOnPage(IWebDriver driver)
        {
            Console.WriteLine("Getting product links");

            // Get the results container
            var resultsContainer = driver.FindElement(By.Id("atfResults"));

            // Find the objects that have the a-row and a-spacing-mini class names
            var productResults = resultsContainer.FindElements(By.CssSelector(".a-row.a-spacing-micro"));

            List<string> strings = new List<string>();
            foreach(var productResult in productResults)
            {
                var text = productResult.FindElement(By.CssSelector("a"));
                var productLink = text.GetAttribute("href");

                Console.WriteLine("Found link: " + productLink);

                strings.Add(productLink);
            }

            return strings;
        }

        public static void NavigateToPage(string url)
        {
            
        }
    }
}
