using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class MensTeesAndTanksSearcher : Searcher
    {
        public override void SearchForProduct(IWebDriver driver, string searchTerm)
        {
            SelectBaseProductCategory(driver);
            SearchUsingAmazonSearchBar(driver, searchTerm);
            SelectMensTeesAndTanks(driver);
        }

        private void SelectMensTeesAndTanks(IWebDriver driver)
        {
            // Navigate to mens section
            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Men")).Click();

            Thread.Sleep(5000);

            // Page reloads. Navigate to clothing section
            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Clothing")).Click();

            Thread.Sleep(5000);

            // Page reloads again. Find t-shirts and tanks
            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("T-Shirts & Tanks")).Click();

            Thread.Sleep(5000);
        }
    }
}
