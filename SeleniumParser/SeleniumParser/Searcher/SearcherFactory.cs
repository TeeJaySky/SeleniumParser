using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class SearcherFactory
    {
        public enum SearcherType
        {
            MensTeesAndTanks
        }

        public static Searcher Make(SearcherType type)
        {
            Searcher returnSearcher;

            switch(type)
            {
                case SearcherType.MensTeesAndTanks:
                    returnSearcher = new MensTeesAndTanksSearcher();
                    break;

                default:
                    returnSearcher = new MensTeesAndTanksSearcher();
                    break;
            }

            return returnSearcher;
        }
    }
}
