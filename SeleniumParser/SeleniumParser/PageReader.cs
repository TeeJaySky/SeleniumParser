using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class PageReader
    {
        public static IEnumerable<BsrRank> GetRankings(IWebDriver driver, string searchTerm, List<string> categoriesToConsider, int maximumBsr)
        {
            List<BsrRank> bsrRanks = new List<BsrRank>();

            try
            {
                // Find the sales rank section on the page - the topmost parent of the list of ranks that apply to the product
                var salesRank = driver.FindElement(By.ClassName("zg_hrsr"));

                // Find the list of items in this container
                var ranks = salesRank.FindElements(By.CssSelector("li"));

                bool imageAlreadySaved = false;

                foreach (var rank in ranks)
                {
                    // The text is in a b tag
                    var categoryName = rank.FindElement(By.CssSelector("b")).Text;

                    // Only consdier if the rank is in one of the specified categories
                    if (categoriesToConsider.Contains(categoryName))
                    {
                        // Trim off the hash from the rank
                        var rankingString = rank.FindElement(By.ClassName("zg_hrsr_rank")).Text.Substring(1);
                        var rankingInt = Convert.ToInt32(rankingString);

                        if (rankingInt <= maximumBsr)
                        {
                            // Store the title for this product now that we want to consider it (delayed DOM query to increase efficiency)
                            var productTitle = driver.FindElement(By.Id("title")).Text;

                            var outputPath = @"C:\Users\Trent\Desktop\TEmp\Outspoken Panda\Images";
                            var fileName = rankingString + "-" + searchTerm + "-" + DateTime.Now.ToString("yyyyMMddhhmmsstt") + ".png";
                            var outputFileName = Path.Combine(outputPath, fileName);

                            if (!imageAlreadySaved)
                            {
                                // Download the image using webclient
                                using (var client = new WebClient())
                                {
                                    var imageWrapper = driver.FindElement(By.Id("imgTagWrapperId"));
                                    var image = imageWrapper.FindElement(By.CssSelector("img"));
                                    var imageSrc = image.GetAttribute("src");

                                    client.DownloadFile(imageSrc, outputFileName);

                                    imageAlreadySaved = true;
                                }
                            }

                            // Add the ranking to the collection that we want to look at
                            bsrRanks.Add(new BsrRank(
                                driver.Url
                                , productTitle
                                , categoryName
                                , rank
                                , rankingString
                                , searchTerm
                                , outputFileName
                            ));

                        }
                    }
                }
            }
            catch (Exception)
            {
                // If the product is really crap, it may not rank on anything
                // Catch exception thrown in this case and just move on...
                //NavLog("Error", ("Exception caught trying to consider " + Driver.Url + ". Exception: " + e.Message);
            }

            return bsrRanks;
        }

        public static List<string> GetProductLinksOnPage(IWebDriver driver)
        {
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

            return strings;
        }

        public static List<string> GetProductLinksOnPageEmptySearchTerm(IWebDriver driver)
        {
            // Get the results container
            var resultsContainer = driver.FindElement(By.Id("mainResults"));

            // Find the objects that have the a-row and a-spacing-mini class names
            var productResults = resultsContainer.FindElements(By.CssSelector(".a-row.a-spacing-micro"));

            List<string> strings = new List<string>();
            foreach (var productResult in productResults)
            {
                var text = productResult.FindElement(By.CssSelector("a"));
                var productLink = text.GetAttribute("href");

                strings.Add(productLink);
            }

            return strings;
        }

        public static bool NextPageButtonExists(IWebDriver driver)
        {
            try
            {
                driver.FindElement(By.Id("pagnNextString"));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
