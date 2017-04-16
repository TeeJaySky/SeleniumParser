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
            , NoveltyMensShirts
            , NoveltyWomensTopAndTees
        }

        public static Searcher Make(SearcherType type)
        {
            Searcher returnSearcher;

            switch(type)
            {
                case SearcherType.MensTeesAndTanks:
                    returnSearcher = new MensTeesAndTanksSearcher();
                    break;

                case SearcherType.NoveltyMensShirts:
                    returnSearcher = new NoveltyMensShirtsSearcher();
                    break;

                case SearcherType.NoveltyWomensTopAndTees:
                    returnSearcher = new NoveltyWomensTopsAndTeesSearcher();
                    break;

                default:
                    throw new InvalidOperationException("Searcher type does not exist: " + type.ToString());
                    break;
            }

            return returnSearcher;
        }
    }
}
