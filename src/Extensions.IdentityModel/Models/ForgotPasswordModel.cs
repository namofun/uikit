using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.IdentityModule.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
