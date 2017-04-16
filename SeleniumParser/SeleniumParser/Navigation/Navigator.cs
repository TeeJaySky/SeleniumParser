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
    public class Navigator
    {
        // 400,000 selling once or twice a week
        // 100,000 selling one design a day
        // 2,000 25-50 a day
        protected int MaximumBSR;
        protected int NumberOfProductsToFind;
        protected int NumberOfPagesToGiveUpAfter;

        protected List<string> CategoriesToConsider;

        protected Navigator(List<string> categoriesToConsider, int maxBsr, int numberOfProductsToFind, int numberOfPagesToSearch)
        {
            CategoriesToConsider = categoriesToConsider;

            MaximumBSR = maxBsr;
            NumberOfProductsToFind = numberOfProductsToFind;
            NumberOfPagesToGiveUpAfter = numberOfPagesToSearch;
        }

        // Override this in child navigators
        public virtual void PerformSearch() { throw new NotImplementedException(); }

        protected bool StopSearchingProduct(IWebDriver driver, int numberOfProductsFound, int numberOfPagesSearched)
        {
            return !PageReader.NextPageButtonExists(driver) ||
                    EnoughProductsHaveBeenFound(numberOfProductsFound) ||
                    EnoughPagesHaveBeenSearched(numberOfPagesSearched);
        }

        protected bool EnoughPagesHaveBeenSearched(int numberOfPagesSearched)
        {
            return numberOfPagesSearched >= NumberOfPagesToGiveUpAfter;
        }

        protected bool EnoughProductsHaveBeenFound(int numberOfProductsFound)
        {
            return numberOfProductsFound >= NumberOfProductsToFind;
        }

        protected void MoveToNextPageOfSearchResults(IWebDriver driver)
        {
            driver.FindElement(By.Id("pagnNextString")).Click();

            DriverUtils.WaitForPageToLoad();
        }

        public void PrintResults(string searchTerm, int numberOfProductsFound, int numberOfPagesSearched)
        {
            Log.Info(searchTerm + ": " + numberOfProductsFound + " products found over " + numberOfPagesSearched + " pages" );
        }
    }
}
