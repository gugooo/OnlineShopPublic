using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Required")]
        [MaxLength(50, ErrorMessage = "MaxLenght")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "InvalidEmail")]
        public string ForgotPasswordEmail { get; set; }
    }
}
