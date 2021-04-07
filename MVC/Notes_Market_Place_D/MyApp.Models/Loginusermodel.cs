using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Models
{
    public class Loginusermodel
    {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }
       
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Required")]
        public string Password { get; set; }

        public String Loginerrormsg { get; set; }
    }
}
