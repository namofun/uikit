using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class SubstrateMvcOptionsConfigurator : IConfigureOptions<MvcOptions>
    {
        private readonly SubstrateApiVisibilityConvention ApiVisibilityConvention;

        public SubstrateMvcOptionsConfigurator(SubstrateApiVisibilityConvention convention)
        {
            ApiVisibilityConvention = convention;
        }

        public void Configure(MvcOptions options)
        {
            options.Conventions.Add(ApiVisibilityConvention);
            options.Conventions.Add(new RemoveInertScpConvention());
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
        }
    }
}
