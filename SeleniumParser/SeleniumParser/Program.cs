using System;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace SeleniumParser
{
    public class Program
    {
        static List<string> CategoriesToConsider = new List<string> { 
            "T-Shirts"
            , "Tank Tops"
            , "T-Shirts & Tanks"
            , "Tanks & Camis" 
        };

        static void Main(string[] args)
        {
            var loggingThread = new Thread(Log.ThreadMain);
            loggingThread.Start();

            Directory.CreateDirectory(@"C:\Users\Trent\Desktop\TEmp\Outspoken Panda\Images");

            Log.Info("Search starting");

            var navigator = NavigationFactory.Make(NavigationFactory.NavigatorType.TopXNavigator, SearcherFactory.SearcherType.MensTeesAndTanks, CategoriesToConsider);
            navigator.PerformSearch();

            Log.Info("Search complete!");

            Log.ShutDown();
        }
    }
}
