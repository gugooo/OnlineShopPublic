using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.ViewModels
{
    public class CheckoutViewModel
    {
        [Display(Name = "Phone")]
        [Required(ErrorMessage = "SharedRequired")]
        [MaxLength(20, ErrorMessage = "SharedMaxLenght")]
        public string Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string Email { get; set; }

        [Display(Name = "Address")]
        [Required(ErrorMessage = "SharedRequired")]
        [MaxLength(100, ErrorMessage = "SharedMaxLenght")]
        public string Address { get; set; }
        
        [Display(Name = "City")]
        [MaxLength(100, ErrorMessage = "SharedMaxLenght")]
        public string City { get; set; }
        
        [Display(Name = "ZIP")]
        [MaxLength(20, ErrorMessage = "SharedMaxLenght")]
        public string ZIP { get; set; }

        [Display(Name = "FName")]
        [MaxLength(40, ErrorMessage = "SharedMaxLenght")]
        public string FirstName { get; set; }

        [Display(Name = "LName")]
        [MaxLength(40, ErrorMessage = "SharedMaxLenght")]
        public string LastName { get; set; }

        [Display(Name = "Description")]
        [MaxLength(200, ErrorMessage = "SharedMaxLenght")]
        public string Description { get; set; }
        public string PromoCode { get; set; }

        [Required(ErrorMessage = "SharedRequired")]
        public string Payment { get; set; }
    }
}
