using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumParser
{
    public class Utils
    {
        static int PageLoadDuration = 0;
        static string OutputFileDir = @"C:\Users\Trent\Desktop\TEmp\Outspoken Panda\";

        /// <summary>
        /// Create the driver and navigate to amazon home
        /// </summary>
        /// <returns></returns>
        public static IWebDriver CreateDriver()
        {
            ChromeOptions options = new ChromeOptions();
            // When launching chrome, configure it not to load images as this will slow down the page load times
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);
            IWebDriver driver = new ChromeDriver(@"C:\Users\Trent\Desktop\Projects\SeleniumParser\SeleniumParser", options);

            driver.Url = "http://www.amazon.com";
            Utils.WaitForPageToLoad();

            return driver;
        }

        public static void WriteBlankLines(int number = 1)
        {
            for (int i = 0; i < number; i ++)
            {
                Console.WriteLine("");
            }
        }

        public static void GoToLink(IWebDriver driver, string url)
        {
            Console.WriteLine("Navigating to: " + url);
            driver.Url = url;

            WaitForPageToLoad();
        }

        public static void WaitForPageToLoad()
        {
            // Wait for page to load
            Thread.Sleep(PageLoadDuration);
        }

        public static void WriteError(string message)
        {
            WriteToFile("Log", DateTime.Now.ToString() + ", Error, " + message);
        }

        public static void WriteToFile(string filename, string value)
        {
            var outputFileName = OutputFileDir + filename + ".txt";
            Console.WriteLine("Writing to " + outputFileName + ": " + value);
            using (var writer = File.AppendText(outputFileName))
            {
                writer.WriteLine(value);
            }
        }

        public static void WriteToFile(string term, SuccessDescriptor result)
        {
            var filename = OutputFileDir + term + ".csv";
            // If the file does not exist add the header columns
            var fileExists = File.Exists(filename);

            using (var writer = File.AppendText(filename))
            {
                if (!fileExists)
                {
                    writer.WriteLine("Title, Category, BSR, URL");
                }

                foreach (var value in result.BestSellerCategoryToRank)
                {
                    writer.WriteLine(result.Title + ", " + value.Key + ", " + value.Value + ", " + result.Url);
                }
            }
        }

        public static void MoveToNextPage(IWebDriver driver)
        {
            driver.FindElement(By.Id("pagnNextString")).Click();

            Utils.WaitForPageToLoad();
        }

        public static string ProcessCategoryName(IWebElement rank, string categoryName)
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
