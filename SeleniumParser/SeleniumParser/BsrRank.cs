using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class BsrRank
    {
        public string Url;
        public string Title;
        public string ProductCategory;
        public string Rank;
        public string SearchTerm;
        public string ImageLocation;

        public BsrRank(string url, string title, string productCategory, IWebElement rankElement, string rank, string searchTerm, string imageLocation)
        {
            Url = RemoveCommas(url);
            Title = RemoveCommas(title);
            Rank = RemoveCommas(rank);
            SearchTerm = RemoveCommas(searchTerm);
            ImageLocation = imageLocation;

            ProductCategory = RemoveCommas(GetDescriptionOfBsrCategory(rankElement, productCategory));

            //Log.Info(title + " considered with ranking " + rank + " for category " + productCategory);
        }

        /// <summary>
        /// Remove unwanted commas from the input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string RemoveCommas(string input)
        {
            return input.Replace(",", "");
        }

        /// <summary>
        /// Gets a better description of the best seller rank category if there are multiple sub categories that have the same name.
        /// I.e. T-Shirts can be found under Men and Women sub categories
        /// </summary>
        /// <param name="rank"></param>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        private static string GetDescriptionOfBsrCategory(IWebElement rank, string categoryName)
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

        /// <summary>
        /// Overload the tostring operator so that we can just dump bsr ranks out without having to format each time
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Log.ToCsv(SearchTerm, Title, ProductCategory, Rank, Url, ImageLocation);
        }  
    }
}
