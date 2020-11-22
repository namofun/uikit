using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Logging extensions for Identity system.
    /// </summary>
    public static class IdentityLoggingExtensions
    {
        /// <summary>
        /// Formats and writes a warning log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="result">The identity result to produce log messages.</param>
        public static void LogWarning(this ILogger logger, IdentityResult result)
        {
            if (result.Succeeded) return;
            var descriptions = result.Errors.Select(e => e.Description);
            descriptions = descriptions.Prepend("An error occurred when finishing identity operations.");
            logger.LogWarning(string.Concat("\r\n", descriptions));
        }

        /// <summary>
        /// Formats and writes an information log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="result">The identity result to produce log messages.</param>
        public static void LogInformation(this ILogger logger, IdentityResult result)
        {
            if (result.Succeeded) return;
            var descriptions = result.Errors.Select(e => e.Description);
            descriptions = descriptions.Prepend("An error occurred when finishing identity operations.");
            logger.LogInformation(string.Concat("\r\n", descriptions));
        }

        /// <summary>
        /// Formats and writes an error log message.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to write to.</param>
        /// <param name="result">The identity result to produce log messages.</param>
        public static void LogError(this ILogger logger, IdentityResult result)
        {
            if (result.Succeeded) return;
            var descriptions = result.Errors.Select(e => e.Description);
            descriptions = descriptions.Prepend("An error occurred when finishing identity operations.");
            logger.LogError(string.Concat("\r\n", descriptions));
        }
    }
}
