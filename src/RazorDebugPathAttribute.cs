using System;

namespace Microsoft.AspNetCore.Razor
{
    /// <summary>
    /// Identify the Razor Debug Path of this assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class RazorDebugPathAttribute : Attribute
    {
        /// <summary>
        /// Gets the Razor Debug Path of this assembly.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Sets the Razor Debug Path of this assembly.
        /// </summary>
        /// <param name="path">The Razor Debug Path.</param>
        public RazorDebugPathAttribute(string path)
        {
            Path = path;
        }
    }
}
