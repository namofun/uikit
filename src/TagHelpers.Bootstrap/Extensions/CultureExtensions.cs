using System.Globalization;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// The culture info.
    /// </summary>
    public static class CultureExtensions
    {
        /// <summary>
        /// Set the current culture info.
        /// </summary>
        /// <param name="builder">The host builder.</param>
        /// <param name="cultureName">The name of culture.</param>
        public static IHostBuilder WithCultureInfo(this IHostBuilder builder, string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            return builder;
        }
    }
}
