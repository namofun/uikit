using System;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// The advanced options for identity module.
    /// </summary>
    public class IdentityAdvancedOptions
    {
        /// <summary>
        /// Gets or sets whether to support two factor authentication.
        /// </summary>
        public bool TwoFactorAuthentication { get; set; }

        /// <summary>
        /// Gets or sets whether to support external login.
        /// </summary>
        public bool ExternalLogin { get; set; }

        /// <summary>
        /// Gets or sets whether to shorten the claim name.
        /// </summary>
        public bool ShortenedClaimName { get; set; }

        /// <summary>
        /// Gets or sets the site name showed in authenticator apps.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the authenticator uri formatter.
        /// <list type="bullet">The first parameter for the user name.</list>
        /// <list type="bullet">The second parameter for the user email.</list>
        /// <list type="bullet">The third parameter for the unformatted authenticator key.</list>
        /// </summary>
        public Func<string, string, string, string> AuthenticatorUriFormat { get; set; }

        /// <summary>
        /// Initialize an instance of <see cref="IdentityAdvancedOptions"/>.
        /// </summary>
        public IdentityAdvancedOptions()
        {
            SiteName = string.Empty;
            AuthenticatorUriFormat = DefaultFormatter;
        }

        /// <summary>
        /// The default formatter for totp qrcode.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="email">The user email.</param>
        /// <param name="unformattedKey">The unformatted key.</param>
        /// <returns><c>otpauth://totp/{SiteName}:{email}?secret={unformattedKey}&amp;issuer={SiteName}&amp;digits=6</c></returns>
        public string DefaultFormatter(string userName, string email, string unformattedKey)
            => string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                UrlEncoder.Default.Encode(SiteName),
                UrlEncoder.Default.Encode(email),
                unformattedKey);
    }
}
