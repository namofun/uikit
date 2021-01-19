using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.IdentityModule.Models
{
    public class ExternalLoginModel
    {
        [Required]
        [UserName]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
