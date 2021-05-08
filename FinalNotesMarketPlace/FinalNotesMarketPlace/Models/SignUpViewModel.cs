﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace FinalNotesMarketPlace.Models
{
    public class SignUpViewModel
    {

        [Required]
        public int ID { get; set; }
        [Required]
        public int RoleID { get; set; }
        [Required(ErrorMessage = "Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Required")]
        [DataType(DataType.EmailAddress)]
        public string EmailID { get; set; }
        [Required(ErrorMessage = "Required")]
        public bool IsEmailVerified { get; set; }

        public bool IsActive { get; set; }
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,24}$", ErrorMessage = "Passord is too weak")]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Not Matched Yet")]
        [Required(ErrorMessage = "Required")]
        public string ConfirmPassword { get; set; }

        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }


    }
}