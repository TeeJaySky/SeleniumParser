using System;
using System.Threading;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumParser
{
    public class Driver
    {
        static int PageLoadDuration = 0;

        /// <summary>
        /// Create the driver and navigate to amazon home
        /// </summary>
        /// <returns></returns>
        public static IWebDriver Make()
        {
            ChromeOptions options = new ChromeOptions();
            // When launching chrome, configure it not to load images as this will slow down the page load times
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            IWebDriver driver = new ChromeDriver(@"C:\Users\Trent\Desktop\Projects\SeleniumParser\SeleniumParser", options);

            driver.Url = "http://www.amazon.com";
            WaitForPageToLoad();

            return driver;
        }

        /// <summary>
        /// Navigate to the specified link
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        public static void GoToLink(IWebDriver driver, string url)
        {
            Console.WriteLine("Navigating to: " + url);
            driver.Url = url;

            WaitForPageToLoad();
        }

        /// <summary>
        /// Blocks the current thread for the configuration wait duration
        /// </summary>
        public static void WaitForPageToLoad()
        {
            // Wait for page to load
            Thread.Sleep(PageLoadDuration);
        }

        /// <summary>
        /// Gets a better description of the best seller rank category if there are multiple sub categories that have the same name.
        /// I.e. T-Shirts can be found under Men and Women sub categories
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static string GetDescriptionOfBsrCategory(IWebElement rank, string categoryName)
        {
            if (categoryName == "T-Shirts")
            {
                // Determine whether or not it is for men or women
                var ladder = rank.FindElement(By.ClassName("zg_hrsr_ladder"));
                var ladderCategories = ladder.FindElements(By.CssSelector("a"));

                foreach (var ladderValue in ladderCategories)
                {
                    if (ladderValue.Text == "Women")
                    {
                        categoryName = "Women's " + categoryName;
                        break;
                    }
                    else if (ladderValue.Text == "Men")
                    {
                        categoryName = "Men's " + categoryName;
                        break;
                    }
                }
            }
            return categoryName;
        }
    }
}
