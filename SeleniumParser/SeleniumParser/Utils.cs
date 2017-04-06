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
        static int PageLoadDuration = 1000;
        static string OutputFileDir = @"C:\Users\Trent\Desktop\TEmp\Outspoken Panda\";

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

        public static void WriteToFile(string term, SuccessDescriptor result)
        {
            using (var writer = File.AppendText(OutputFileDir + term + ".txt"))
            {
                foreach (var value in result.BestSellerCategoryToRank)
                {
                    writer.WriteLine(value.Key + ", " + value.Value + ": " + result.Url);
                }
            }
        }

        public static void WriteLinksToFileForSearchTerm(string term, List<string> productLinks)
        {
            using (var writer = File.AppendText(OutputFileDir + term + ".txt"))
            {
                writer.WriteLine("Links for " + term);
                foreach (var link in productLinks)
                {
                    writer.WriteLine(link);
                }
            }
        }

        public static void MoveToNextPage(IWebDriver driver)
        {
            driver.FindElement(By.Id("pagnNextString")).Click();

            Utils.WaitForPageToLoad();
        }
    }
}
