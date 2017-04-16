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
    public class BaseSearcher
    {
        public IWebDriver Driver;

        public BaseSearcher(IWebDriver driver)
        {
            Driver = driver;
        }

        public virtual void SearchForProduct(string searchTerm){}

        public void SelectBaseProductCategory(string productCategory = "Clothing, Shoes & Jewelry")
        {
            // Ensure that we are searching with the right category. Each category apparently has different page formatting
            var categoryDropdown = Driver.FindElement(By.ClassName("nav-search-dropdown"));
            var selectElementDropdown = new SelectElement(categoryDropdown);

            // Select the required category
            selectElementDropdown.SelectByText(productCategory);
        }

        /// <summary>
        /// Chooses product category, and then searches for the specified term by typing into search box and clicking the search button
        /// </summary>
        public void SearchUsingAmazonSearchBar(string searchTerm)
        {
            // Get the search box
            IWebElement searchBox = Driver.FindElement(By.Id("twotabsearchtextbox"));

            searchBox.Clear();

            // Search for the term
            searchBox.SendKeys(searchTerm);

            // Click the search button
            var searchButton = Driver.FindElement(By.ClassName("nav-input"));
            searchButton.Click();
        }
    }

    public class MensTeesAndTanksSearcher : BaseSearcher
    {
        public MensTeesAndTanksSearcher(IWebDriver driver) : base(driver)
        {
        }

        public override void SearchForProduct(string searchTerm)
        {
            SelectBaseProductCategory();
            SearchUsingAmazonSearchBar(searchTerm);
            SelectMensTeesAndTanks();
        }

        private void SelectMensTeesAndTanks()
        {
            // Navigate to mens section
            Driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Men")).Click();

            Thread.Sleep(5000);

            // Page reloads. Navigate to clothing section
            Driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Clothing")).Click();

            Thread.Sleep(5000);

            // Page reloads again. Find t-shirts and tanks
            Driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("T-Shirts & Tanks")).Click();

            Thread.Sleep(5000);
        }
    }
}

