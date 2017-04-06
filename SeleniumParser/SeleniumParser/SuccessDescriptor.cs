using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumParser
{
    public class SuccessDescriptor
    {
        public Dictionary<string, int> BestSellerCategoryToRank = new Dictionary<string,int>();
        public string Url;

        public SuccessDescriptor(string url)
        {
            Url = url;
        }
    }
}
