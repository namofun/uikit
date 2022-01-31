using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Identity;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceAuthHttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds an additional message handler from the dependency injection container for
        /// a named <see cref="HttpClient"/> that resolves Azure authentication tokens.
        /// </summary>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <param name="scopes">The API scopes required by this service.</param>
        /// <returns>An <see cref="IHttpClientBuilder"/> that can be used to configure the client.</returns>
        public static IHttpClientBuilder AddAzureAuthHandler(this IHttpClientBuilder builder, string[] scopes)
        {
            if (scopes == null || scopes.Length == 0) throw new ArgumentNullException(nameof(scopes));
            builder.Services.TryAddSingleton<ITokenCredentialProvider, DefaultAzureCredentialProvider>();
            return builder.AddHttpMessageHandler(TokenCredentialDelegatingHandler.Factory(scopes));
        }
    }
}
