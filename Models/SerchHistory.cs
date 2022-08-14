using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.Models
{
    public class SerchHistory
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Key { get; set; }
        public virtual User User { get; set; }
    }
}
