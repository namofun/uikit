using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    /// <summary>
    /// Provides several misc extension methods.
    /// </summary>
    public static class MiscTestsExtensions
    {
        /// <remarks>To suppress the call to <see cref="IDisposable.Dispose"/></remarks>
        private class DefaultAccessor : IApplicationAccessor
        {
            public SubstrateApplicationBase Instance { get; }

            public DefaultAccessor(SubstrateApplicationBase @base)
            {
                Instance = @base;
            }
        }

        /// <summary>
        /// Disable the MigrationAssembly because we are in tests.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <param name="base">The substrate application to test</param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder MarkTest(this IHostBuilder builder, SubstrateApplicationBase @base)
        {
            builder.Properties["ShouldNotUseMigrationAssembly"] = true;
            builder.ConfigureServices(svcs => svcs.AddSingleton<IApplicationAccessor>(new DefaultAccessor(@base)));
            return builder;
        }

        /// <summary>
        /// A <typeparamref name="T"/> representation of the JSON value.
        /// </summary>
        /// <typeparam name="T">The target type of the JSON value.</typeparam>
        /// <param name="httpContent">The HTTP content.</param>
        /// <param name="options">The JSON serializer options.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public static async Task<T> ReadAsJsonAsync<T>(
            this HttpContent httpContent,
            JsonSerializerOptions? options = null)
        {
            using var stream = await httpContent.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, options);
        }

        /// <summary>
        /// Do asynchronous operations in scoped service context.
        /// </summary>
        /// <param name="services">The service provider to create the scope.</param>
        /// <param name="scopeContent">The scope runner.</param>
        /// <returns>The task for running the scoped operation.</returns>
        public static async Task RunScoped(
            this SubstrateApplicationBase services,
            Func<IServiceProvider, Task> scopeContent)
        {
            using var scope = services.Services.CreateScope();
            await scopeContent.Invoke(scope.ServiceProvider);
        }

        /// <summary>
        /// Do synchronous operations in scoped service context.
        /// </summary>
        /// <param name="services">The service provider to create the scope.</param>
        /// <param name="scopeContent">The scope runner.</param>
        public static void RunScoped(
            this SubstrateApplicationBase services,
            Action<IServiceProvider> scopeContent)
        {
            using var scope = services.Services.CreateScope();
            scopeContent.Invoke(scope.ServiceProvider);
        }

        /// <summary>
        /// Login the http client.
        /// </summary>
        /// <param name="client">The http client.</param>
        /// <param name="Username">The username to login.</param>
        /// <param name="Password">The password to login.</param>
        /// <param name="RememberMe">The remember me option.</param>
        /// <returns>Whether login succeeded.</returns>
        public static async Task<bool> LoginAsync(
            this HttpClient client,
            string Username, string Password, bool RememberMe = false)
        {
            string __RequestVerificationToken;
            using (var root = await client.GetAsync("/account/login?returnUrl=%2F"))
            {
                var body = await root.Content.ReadAsStringAsync();
                const string flag = "<input name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
                var idx = body.IndexOf(flag) + flag.Length;
                var idxEnd = body.IndexOf('"', idx);
                __RequestVerificationToken = body[idx..idxEnd];
            }

            using (var root = await client.PostAsync(
                "/account/login?returnUrl=%2F",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    [nameof(Username)] = Username,
                    [nameof(Password)] = Password,
                    [nameof(__RequestVerificationToken)] = __RequestVerificationToken,
                    [nameof(RememberMe)] = RememberMe.ToString().ToLower(),
                })))
            {
                var content = await root.Content.ReadAsStringAsync();
                return !content.Contains("Invalid login attempt.");
            }
        }

        private static readonly Func<WebApplicationFactoryClientOptions, DelegatingHandler[]> CreateHandlersDelegate =
            typeof(WebApplicationFactoryClientOptions)
                .GetMethod(nameof(CreateHandlers), BindingFlags.Instance | BindingFlags.NonPublic)!
                .CreateDelegate(typeof(Func<WebApplicationFactoryClientOptions, DelegatingHandler[]>)) as dynamic;

        internal static DelegatingHandler[] CreateHandlers(this WebApplicationFactoryClientOptions options) =>
            CreateHandlersDelegate(options);
    }
}
