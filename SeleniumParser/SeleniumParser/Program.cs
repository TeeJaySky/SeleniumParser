using System;
using System.Collections.Generic;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class Program
    {
        static List<string> SearchTerms = new List<string> { 
            "cat t shirt"
            , "motivational t shirt"
            , "motivational exercise t shirt"
            , "saying t shirt"
            , "donald trump t shirt"
            , "anti donald trump t shirt"
            , "political t shirt"
            , "cat saying t shirt"
            , "dog saying t shirt"
            , "funny saying t shirt"
            , "french bulldog t shirt"
            , "pug t shirt"
            , "funny saying t shirt"
            , "coffee t shirt"
            , "tea t shirt"
            , "doughnut t shirt"
            , "salty t shirt"
            , "salty saying t shirt"
            , "snatched t shirt"
            , "werq t shirt"
            , "slay t shirt"
            , "yolo t shirt"
            , "a crapella t shirt"
            , "amazeballs t shirt"
            , "askhole t shirt"
            , "awesome sauce t shirt"
            , "baby bump t shirt"
            , "badassery t shirt"
            , "beer me t shirt"
            , "blamestorming t shirt"
            , "bootylicious t shirt"
            , "bromance t shirt"
            , "bye felicia t shirt"
            , "cougar t shirt"
            , "designated drunk t shirt"
            , "duck face t shirt"
            , "fanboi t shirt"
            , "fan girl t shirt"
            , "fauxpology t shirt"
            , "foodie t shirt"
            , "frak t shirt"
            , "friend zone t shirt"
            , "fro yo t shirt"
            , "fro-yo t shirt"
            , "gaydar t shirt"
            , "girl crush t shirt"
            , "grrrl t shirt"
            , "hangry t shirt"
            , "hipster t shirt"
            , "hot mess t shirt"
            , "humblebrag t shirt"
            , "jailbait t shirt"
            , "knosh t shirt"
            , "lol t shirt"
            , "make it rain t shirt"
            , "man boobs t shirt"
            , "man cave t shirt"
            , "man sweats t shirt"
            , "milf t shirt"
            , "netflix and chill t shirt"
            , "ninja sex t shirt"
            , "nom t shirt"
            , "nontroversy t shirt"
            , "nsfw t shirt"
            , "party foul t shirt"
            , "phat t shirt"
            , "pregret t shirt"
            , "pwned t shirt"
            , "quantum physics t shirt"
            , "ratchet t shirt"
            , "rendezbooz t shirt"
            , "rickroll t shirt"
            , "said no one ever t shirt"
            , "selfie t shirt"
            , "sext t shirt"
            , "side boob t shirt"
            , "that's what she said t shirt"
            , "trout t shirt"
            , "twerk t shirt"
            , "typeractive t shirt"
            , "wtf t shirt"
            , "yolo t shirt"
            , "zombie t shirt"
            , "sus t shirt"
            , "boots t shirt"
            , "hunty t shirt"
            , "savage t shirt"
            , "thirsty t shirt"
            , "Hillary t shirt"
        };

        static List<string> CategoriesToConsider = new List<string> { 
            "T-Shirts"
            , "Tank Tops"
            , "T-Shirts & Tanks"
            , "Tanks & Camis" 
        };

        static string SearchCategory = "Clothing, Shoes & Jewelry";

        static void Main(string[] args)
        {
            var loggingThread = new Thread(Log.ThreadMain);
            loggingThread.Start();

            Log.Info("Search starting");

            // One collection of navigators
            List<AmazonNavigator> navigators = new List<AmazonNavigator>();

            List<Task> tasks = new List<Task>();

            // Initial group preparation
            int searchTermIndex;
            const int maximumNumberOfConcurrentTasks = 15;
            for(searchTermIndex = 0; searchTermIndex < Math.Min(maximumNumberOfConcurrentTasks, SearchTerms.Count); searchTermIndex ++)
            {
                Task task = CreateAmazonNavigator(searchTermIndex);

                tasks.Add(task);
            }

            bool finishedAddingTasks = false;
            while (!finishedAddingTasks)
            {
                // Wait for any task to finish
                var completedTaskIndex = Task.WaitAny(tasks.ToArray());
                tasks.RemoveAt(completedTaskIndex);

                searchTermIndex++;

                // Determine if we still have more search terms to look at
                if(searchTermIndex < SearchTerms.Count)
                {
                    Log.Info("Adding new task to maintain 10 active searches");

                    // Add the next term to the list
                    tasks.Add(CreateAmazonNavigator(searchTermIndex));
                }
                else
                {
                    finishedAddingTasks = true;
                }
            }

            Log.Info("Finished adding tasks. Waiting for remaining tasks to complete");

            // We have no more tasks to add, so just wait for the rest to finish
            Task.WaitAll(tasks.ToArray());

            Log.ShutDown();

            Log.Info("Search complete!");
        }

        private static Task CreateAmazonNavigator(int searchTermIndex)
        {
            // Create the amazon navigator as an asynchronous task
            Task task = Task.Run(() =>
            {
                var amazonNavigator = new AmazonNavigator(SearchTerms[searchTermIndex], CategoriesToConsider, SearchCategory);
                amazonNavigator.PerformSearch();
            });
            return task;
        }
    }
}
