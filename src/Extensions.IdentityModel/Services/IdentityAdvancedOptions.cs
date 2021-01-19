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
    }
}
