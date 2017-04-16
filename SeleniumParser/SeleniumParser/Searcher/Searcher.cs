using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class Searcher
    {
        public virtual void SearchForProduct(IWebDriver driver, string searchTerm){}

        public void SelectBaseProductCategory(IWebDriver driver, string productCategory = "Clothing, Shoes & Jewelry")
        {
            // Ensure that we are searching with the right category. Each category apparently has different page formatting
            var categoryDropdown = driver.FindElement(By.ClassName("nav-search-dropdown"));
            var selectElementDropdown = new SelectElement(categoryDropdown);

            // Select the required category
            selectElementDropdown.SelectByText(productCategory);
        }

        /// <summary>
        /// Chooses product category, and then searches for the specified term by typing into search box and clicking the search button
        /// </summary>
        public void SearchUsingAmazonSearchBar(IWebDriver driver, string searchTerm)
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
    }
}

