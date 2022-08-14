using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class ProductAjax
    {
        public int CatalogId { get; set; }
        public int ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public string Title { get; set; }
        public string Brand { get; set; }
        public string Description { get; set; }
        public string SerchKeys { get; set; }
        public string Price { get; set; }
        public string Sale { get; set; }
        public bool IsActive { get; set; }
        public bool IsMine { get; set; }
        public IEnumerable<int> RemoveIMGsId { get; set; }
        public IEnumerable<ProductAtributesAjax> ProductAtributes { get; set; }
        public int CurrentAtributeValue { get; set; }
        public int GroupAtribute { get; set; }
        public IEnumerable<int> AtributeLinks { get; set; }
        public int FirstAtributeId { get; set; }
    }
    public class ProductAtributesAjax
    {
        public IEnumerable<string> AtributeValuesID { get; set; }
        public int Quantity { get; set; }
    }
}
