using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace MyApp.Models
{
     public class fpmodel
     {
        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Enter Email address")]
        public string EmailID { get; set; }
     }
}
