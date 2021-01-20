using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace SatelliteSite
{
    /// <summary>
    /// Options for configuring authentication using Azure Active Directory.
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Gets or sets the client Id.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure Active Directory instance.
        /// </summary>
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the domain of the Azure Active Directory tenant.
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sign in callback path.
        /// </summary>
        public string CallbackPath { get; set; } = "/signin-oidc";
    }

    public static class AzureAdAuthentication
    {
        public static AuthenticationBuilder AddAzureAd(
            this AuthenticationBuilder builder,
            Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure<AzureAdOptions>(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.AddOpenIdConnect("AzureAD", "Azure Active Directory", options => { });
            builder.AddOpenIdConnect("AzureAD2", "Azure Active Directory 2", options => { });
            builder.AddOpenIdConnect("AzureAD3", "Azure Active Directory 3", options => { });
            return builder;
        }

        private class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> azureOptions)
            {
                _azureOptions = azureOptions.Value;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                if (!name.StartsWith("AzureAD")) return;

                options.ClientId = _azureOptions.ClientId;
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}";
                options.UseTokenLifetime = true;
                options.CallbackPath = _azureOptions.CallbackPath;
                options.RequireHttpsMetadata = false;
                options.ClientSecret = _azureOptions.ClientSecret;
                options.Scope.Add("email");
            }

            public void Configure(OpenIdConnectOptions options)
            {
            }
        }
    }
}
