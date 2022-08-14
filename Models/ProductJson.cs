using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class ProductJson
    {
        public int id { get; set; }
        public int count { get; set; }
        public IEnumerable<int> atrs { get; set; }
    }
}
