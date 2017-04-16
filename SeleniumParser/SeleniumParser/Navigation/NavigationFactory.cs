using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class NavigationFactory
    {
        public enum NavigatorType
        {
            SearchTermBased
            , TopXNavigator
        }

        public static Navigator Make(NavigatorType type, SearcherFactory.SearcherType searcher, List<string> categoriesToConsider)
        {
            Navigator returnNavigator;

            switch(type)
            {
                case NavigatorType.SearchTermBased:
                    returnNavigator = new SearchBasedNavigator(searcher, categoriesToConsider);
                    break;
                case NavigatorType.TopXNavigator:
                    returnNavigator = new TopXNavigator(SearcherFactory.Make(searcher), categoriesToConsider);
                    break;
                default:
                    returnNavigator = new SearchBasedNavigator(searcher, categoriesToConsider);
                    break;
            }

            return returnNavigator;
        }
    }
}
