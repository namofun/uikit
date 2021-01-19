using System.Collections.Generic;

namespace SatelliteSite.IdentityModule.Models
{
    public class TwoFactorAuthenticationModel
    {
        public bool HasAuthenticator { get; set; }

        public IReadOnlyList<string> RecoveryCodes { get; set; }

        public bool Is2faEnabled { get; set; }
    }
}
