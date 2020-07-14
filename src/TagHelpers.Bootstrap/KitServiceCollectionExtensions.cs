using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
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
        /// Conventions for endpoints that requires the authorization.
        /// </summary>
        /// <param name="builder">The endpoint convention builder.</param>
        /// <param name="roles">The roles to be confirmed.</param>
        /// <returns>The endpoint convention builder to chain the configurations.</returns>
        public static IEndpointConventionBuilder RequireRoles(this IEndpointConventionBuilder builder, string roles)
        {
            return builder.RequireAuthorization(new AuthorizeAttribute { Roles = roles });
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

        /// <summary>
        /// Batch add the application parts into the <see cref="ApplicationPartManager"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure more.</param>
        /// <param name="parts">The list of <see cref="ApplicationPart"/>.</param>
        /// <returns>The <see cref="IMvcBuilder"/> to chain the conventions.</returns>
        public static IMvcBuilder AddApplicationParts(this IMvcBuilder builder, IEnumerable<ApplicationPart> parts)
        {
            return builder.ConfigureApplicationPartManager(apm =>
            {
                foreach (var part in parts)
                {
                    apm.ApplicationParts.Add(part);
                }
            });
        }

        /// <summary>
        /// Batch add the file providers into the razor source file digging chain.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure more.</param>
        /// <param name="fileProviders">The list of <see cref="IFileProvider"/>.</param>
        /// <param name="add">Whether to apply this convention.</param>
        /// <returns>The <see cref="IMvcBuilder"/> to chain the conventions.</returns>
        public static IMvcBuilder AddRazorRuntimeCompilation(this IMvcBuilder builder, IFileProvider? fileProvider, bool add = true)
        {
            if (!add || fileProvider == null) return builder;
            return builder.AddRazorRuntimeCompilation(options =>
            {
                options.FileProviders.Add(fileProvider);
            });
        }

        /// <summary>
        /// Filter out the <see cref="NotFoundFileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">The source <see cref="IFileInfo"/>.</param>
        /// <returns>The <see cref="IFileInfo"/>.</returns>
        internal static IFileInfo? NullIfNotFound(this IFileInfo fileInfo)
        {
            if (fileInfo == null || fileInfo is NotFoundFileInfo || !fileInfo.Exists) return null;
            return fileInfo;
        }

        /// <summary>
        /// Filter out the <see cref="NullChangeToken"/>.
        /// </summary>
        /// <param name="changeToken">The source <see cref="IChangeToken"/>.</param>
        /// <returns>The <see cref="IChangeToken"/>.</returns>
        internal static IChangeToken? NullIfNotFound(this IChangeToken changeToken)
        {
            if (changeToken == null || changeToken is NullChangeToken) return null;
            return changeToken;
        }
    }
}
