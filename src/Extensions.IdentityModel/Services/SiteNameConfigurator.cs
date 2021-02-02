using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace SatelliteSite.IdentityModule
{
    internal class SubstrateSiteNameConfigurator : IConfigureOptions<IdentityAdvancedOptions>
    {
        private readonly SubstrateOptions _options;

        public SubstrateSiteNameConfigurator(IOptions<SubstrateOptions> options)
        {
            _options = options.Value;
        }

        public void Configure(IdentityAdvancedOptions options)
        {
            if (string.IsNullOrEmpty(options.SiteName))
            {
                options.SiteName = _options.SiteName;
            }
        }
    }
}
