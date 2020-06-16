using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Linq;
using System.Text.Json.Serialization;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Contains several static extension methods to help build <see cref="IServiceCollection"/>.
    /// </summary>
    public static class KitServiceCollectionExtensions
    {
        /// <summary>
        /// Add the <see cref="TimeSpanJsonConverter"/> into json serializer options.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/></param>
        /// <returns>The <see cref="IMvcBuilder"/></returns>
        public static IMvcBuilder AddTimeSpanJsonConverter(this IMvcBuilder builder)
        {
            return builder.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter()));
        }

        /// <summary>
        /// Add the <see cref="SlugifyParameterTransformer"/> into route token transformer conventions.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/></param>
        /// <returns>The <see cref="IMvcBuilder"/></returns>
        public static IMvcBuilder UseSlugifyParameterTransformer(this IMvcBuilder builder)
        {
            builder.Services.Configure<MvcOptions>(options =>
                options.Conventions.Add(
                    new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
            return builder;
        }

        /// <summary>
        /// Replace the default <see cref="LinkGenerator"/> implemention with more routing token prob.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/></param>
        /// <returns>The <see cref="IMvcBuilder"/></returns>
        public static IMvcBuilder ReplaceDefaultLinkGenerator(this IMvcBuilder builder)
        {
            var old = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(LinkGenerator));
            OrderLinkGenerator.typeInner = old.ImplementationType;
            builder.Services.Replace(ServiceDescriptor.Singleton<LinkGenerator, OrderLinkGenerator>());
            return builder;
        }
    }
}
