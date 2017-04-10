using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumParser
{
    public class DriverUtils
    {
        static int PageLoadDuration = 0;

        /// <summary>
        /// Create the driver and navigate to amazon home
        /// </summary>
        /// <returns></returns>
        public static IWebDriver Make()
        {
            bool madeDriver = false;

            ChromeOptions options = new ChromeOptions();
            // When launching chrome, configure it not to load images as this will slow down the page load times
            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

            IWebDriver driver = null;

            int creationAttempts = 0;
            const int maxDriverCreationAttempts = 100;

            while(!madeDriver)
            {
                try
                {
                    driver = new ChromeDriver(@"C:\Users\Trent\Desktop\Projects\SeleniumParser\SeleniumParser", options, TimeSpan.FromMinutes(10));
                    driver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(10);

                    driver.Url = "http://www.amazon.com";
                    WaitForPageToLoad();

                    madeDriver = true;
                }
                catch
                {
                    creationAttempts++;

                    Log.Error("Couldn't make driver. Attempt number " + creationAttempts);

                    if(creationAttempts == maxDriverCreationAttempts)
                    {
                        throw new Exception("Couldnt make driver in " + creationAttempts + " attempts");
                    }
                    // Don't do anything - just keep trying to make that god damn driver!!
                }
            }

            return driver;
        }

        /// <summary>
        /// Navigate to the specified link
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="url"></param>
        public static void GoToLink(IWebDriver driver, string url)
        {
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
    }
}
