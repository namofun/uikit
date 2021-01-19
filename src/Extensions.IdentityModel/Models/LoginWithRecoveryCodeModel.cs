using System.ComponentModel.DataAnnotations;

namespace SatelliteSite.IdentityModule.Models
{
    public class LoginWithRecoveryCodeModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }
}
