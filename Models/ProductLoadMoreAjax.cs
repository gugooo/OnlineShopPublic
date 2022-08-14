using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class ProductLoadMoreAjax
    {
        public int id { get; set; }
        public int index { get; set; }
        public string brand { get; set; }
        public string serchKeys { get; set; }
        public decimal min { get; set; }
        public decimal max { get; set; }
        public IEnumerable<int> atributeIds { get; set; }
    }
}
