using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MyApp.Models
{
    
        public class Cpm
        {
            [Required(ErrorMessage = "Required")]
            public string OldPassword { get; set; }
            [Required(ErrorMessage = "Required")]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,24}$", ErrorMessage = "Password must be between 8 and 24 characters and contain one uppercase letter, one lowercase letter, one digit and one special character.")]
            [DisplayName("New Password")]
            public string NewPassword { get; set; }
            [Required(ErrorMessage = "Required")]
            [Compare("NewPassword", ErrorMessage = "password is Not Matched Yet")]
            [DisplayName("Confirm Password")]
            public string NewConfirmPassword { get; set; }
        }
    
}
