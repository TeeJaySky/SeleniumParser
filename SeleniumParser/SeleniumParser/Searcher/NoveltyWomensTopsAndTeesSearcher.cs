using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    class NoveltyWomensTopsAndTeesSearcher : Searcher
    {
        public override void SearchForProduct(IWebDriver driver, string searchTerm)
        {
            SelectBaseProductCategory(driver);
            SearchUsingAmazonSearchBar(driver, searchTerm);
            SelectMensNoveltyShirts(driver);
        }

        private void SelectMensNoveltyShirts(IWebDriver driver)
        {
            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Novelty & More")).Click();

            Thread.Sleep(5000);

            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Clothing")).Click();

            Thread.Sleep(5000);

            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Novelty")).Click();

            Thread.Sleep(5000);

            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Women")).Click();

            Thread.Sleep(5000);

            driver.FindElement(By.ClassName("categoryRefinementsSection"))
                .FindElement(By.LinkText("Tops & Tees")).Click();

            Thread.Sleep(5000);
        }
    }
}
