using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class SubstrateMvcOptionsConfigurator : IConfigureOptions<MvcOptions>
    {
        private readonly SubstrateApiVisibilityConvention _apiVisibilityConvention;
        private readonly IEnumerable<FeatureAvailabilityConvention> _featureAvailabilityConventions;

        public SubstrateMvcOptionsConfigurator(
            SubstrateApiVisibilityConvention convention,
            IEnumerable<FeatureAvailabilityConvention> featureAvailabilityConventions)
        {
            _apiVisibilityConvention = convention;
            _featureAvailabilityConventions = featureAvailabilityConventions;
        }

        public void Configure(MvcOptions options)
        {
            options.Conventions.Add(_apiVisibilityConvention);
            options.Conventions.Add(new RemoveInertScpConvention());
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));

            foreach (var convention in _featureAvailabilityConventions)
            {
                options.Conventions.Add(convention);
            }
        }
    }
}
