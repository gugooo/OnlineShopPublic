using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class Atribute
    {
        public Atribute()
        {
            CultureName = new HashSet<CultureData>();
            Values = new HashSet<AtributeValue>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<CultureData> CultureName { get; set; }
        public virtual ICollection<AtributeValue> Values { get; set; }
    }
    public class AtributeValue
    {
        public AtributeValue()
        {
            CultureName = new HashSet<CultureData>();
        }
        public int Id { get; set; }
        public string Value { get; set; }
        public virtual ICollection<CultureData> CultureName { get; set; }
        public virtual Atribute FK_Atribute { get; set; }
    }
}
