using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.ViewModels.Admin
{
    public class ReviewViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; }
        public bool? Status { get; set; }
    }
}
