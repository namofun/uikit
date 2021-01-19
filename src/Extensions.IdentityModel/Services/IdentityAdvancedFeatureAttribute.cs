using Microsoft.AspNetCore.Identity;
using System;

namespace SatelliteSite.IdentityModule
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class IdentityAdvancedFeatureAttribute : Attribute
    {
        public string Name { get; }

        public IdentityAdvancedFeatureAttribute(string name)
        {
            Name = name;
        }

        public bool GetActivated(IdentityAdvancedOptions options)
        {
            return Name switch
            {
                nameof(options.ExternalLogin) => options.ExternalLogin,
                nameof(options.TwoFactorAuthentication) => options.TwoFactorAuthentication,
                _ => throw new NotSupportedException("Feature not specified."),
            };
        }
    }
}
