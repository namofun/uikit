namespace SatelliteSite.Services
{
    /// <summary>
    /// The display options for Application Insights.
    /// </summary>
    /// <remarks>Learn more on <a href="https://dev.applicationinsights.io/">Azure Application Insights REST API</a>.</remarks>
    public class ApplicationInsightsDisplayOptions
    {
        /// <summary>
        /// The application ID
        /// </summary>
        public string ApplicationId { get; set; } = string.Empty;

        /// <summary>
        /// The API key
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;
    }
}
