using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AlcantaraNew.ViewModels
{
    public class ResetPasswordViewModel
    {

        [Required(ErrorMessage = "SharedRequired")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        [MaxLength(25, ErrorMessage = "SharedMaxLenght")]
        [MinLength(6, ErrorMessage = "SharedMinLenght")]
        public string ResetPassword { get; set; }

        [Required(ErrorMessage = "SharedRequired")]
        [DataType(DataType.Password)]
        [Compare("ResetPassword", ErrorMessage = "ComparePasswordErrMassage")]
        [Display(Name = "ComparePassword")]
        [MaxLength(25, ErrorMessage = "SharedMaxLenght")]
        [MinLength(6, ErrorMessage = "SharedMinLenght")]
        public string CompareResetPassword { get; set; }

        [Required]
        [MaxLength(100)]
        public string EmailID { get; set; }

        [Required]
        [MaxLength(300)]
        public string Code { get; set; }
    }
}
