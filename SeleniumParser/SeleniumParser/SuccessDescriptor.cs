using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class SuccessDescriptor
    {
        public string Url;
        public string Title;
        public Dictionary<string, int> BestSellerCategoryToRank = new Dictionary<string, int>();

        public SuccessDescriptor(string url)
        {
            Url = url;
        }
    }
}
