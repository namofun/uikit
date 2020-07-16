﻿using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.IdentityModule.Models
{
    public class LoginModel
    {
        [Required]
        [UserName]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
