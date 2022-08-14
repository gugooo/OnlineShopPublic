using AlcantaraNew.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.ViewModels
{
    public class ProductView
    {
        public int Id { get; set; }
        public int ImgId { get; set; }
        public int ImgId2 { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public string Price { get; set; }
        public string Sale { get; set; }
        public string Brand { get; set; }
        public IDictionary<Atribute, List<AtributeValue>> Atributes { get; set; }
        public IEnumerable<int> Imgs { get; set; }
        public AtributeValue LinkAtrVal { get; set; }
    }
}
