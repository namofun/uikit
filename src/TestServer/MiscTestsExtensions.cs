using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            this IServiceProvider services,
            Func<IServiceProvider, Task> scopeContent)
        {
            using var scope = services.CreateScope();
            await scopeContent.Invoke(scope.ServiceProvider);
        }

        /// <summary>
        /// Do synchronous operations in scoped service context.
        /// </summary>
        /// <param name="services">The service provider to create the scope.</param>
        /// <param name="scopeContent">The scope runner.</param>
        public static void RunScoped(
            this IServiceProvider services,
            Action<IServiceProvider> scopeContent)
        {
            using var scope = services.CreateScope();
            scopeContent.Invoke(scope.ServiceProvider);
        }

        private static readonly Func<WebApplicationFactoryClientOptions, DelegatingHandler[]> CreateHandlersDelegate =
            typeof(WebApplicationFactoryClientOptions)
                .GetMethod(nameof(CreateHandlers), BindingFlags.Instance | BindingFlags.NonPublic)!
                .CreateDelegate(typeof(Func<WebApplicationFactoryClientOptions, DelegatingHandler[]>)) as dynamic;

        internal static DelegatingHandler[] CreateHandlers(this WebApplicationFactoryClientOptions options) =>
            CreateHandlersDelegate(options);
    }
}
