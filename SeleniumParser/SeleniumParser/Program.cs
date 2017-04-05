using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumParser
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver(@"C:\Users\Trent\Desktop\Projects\SeleniumParser\SeleniumParser");
            driver.Url = "http://www.amazon.com";
            // Implicitly wait for 10 seconds before elements are declared as non-existant on page
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            SearchUsingAmazonSearchBar(driver, "cat t shirt");

            List<string> productLinks = new List<string>();

            // Click the next button to move to the next page
            for (int i = 0; i < 10; i ++)
            {
                // Todo: stop trying to click the next button when it is not loaded on the page. Alternatively, set the url based off modifying it to look at a specific page number
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.Until(drv => drv.FindElement(By.Id("pagnNextString"))).Click();
            }
            //productLinks.AddRange(GetProductLinksOnPage(driver));

            Console.ReadKey();

            // Close the browser
            driver.Close();
        }

        private static void SearchUsingAmazonSearchBar(IWebDriver driver, string searchTerm)
        {
            // Get the search box
            IWebElement searchBox = driver.FindElement(By.Id("twotabsearchtextbox"));
            searchBox.Clear();
            searchBox.SendKeys(searchTerm);

            // Get the search button
            var searchButton = driver.FindElement(By.ClassName("nav-input"));
            searchButton.Click();
        }

        private static List<string> GetProductLinksOnPage(IWebDriver driver)
        {
            Console.WriteLine("Getting product links");

            // Get the results container
            var resultsContainer = driver.FindElement(By.Id("atfResults"));

            // Find the objects that have the a-row and a-spacing-mini class names
            var productResults = resultsContainer.FindElements(By.CssSelector(".a-row.a-spacing-mini"));

            List<string> strings = new List<string>();
            foreach(var productResult in productResults)
            {
                var text = productResult.FindElement(By.CssSelector("a"));
                var productLink = text.GetAttribute("href");
                strings.Add(productLink);
            }

            return strings;
        }

        private static void NavigateToPage(string url)
        {
            
        }
    }
}
