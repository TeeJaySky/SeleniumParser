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
            Log.Info("Search starting");

            // One collection of navigators
            List<AmazonNavigator> navigators = new List<AmazonNavigator>();

            // Parallelise the navigation
            Parallel.For(
                0
                , SearchTerms.Count
                , new ParallelOptions { MaxDegreeOfParallelism = 30 }
                , i => {
                var amazonNavigator = new AmazonNavigator(SearchTerms[i], CategoriesToConsider, SearchCategory);
                amazonNavigator.PerformSearch();
            });

            Log.Info("Search complete!");

            for(int i = 0; i < SearchTerms.Count; i ++)
            {
                Log.Info(navigators[i].ToString());
            }
        }
    }
}
