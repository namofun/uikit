namespace System.Globalization
{
    /// <summary>
    /// The culture info.
    /// </summary>
    public static class Culture
    {
        /// <summary>
        /// Set the current culture info.
        /// </summary>
        /// <param name="cultureName">The name of culture.</param>
        public static void SetCultureInfo(string cultureName)
        {
            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
