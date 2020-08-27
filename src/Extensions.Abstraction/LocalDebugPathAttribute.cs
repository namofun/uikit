namespace System
{
    /// <summary>
    /// Identify the local debug path of this assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class LocalDebugPathAttribute : Attribute
    {
        /// <summary>
        /// Gets the local debug path of this assembly.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Sets the local debug path of this assembly.
        /// </summary>
        /// <param name="path">The local debug path.</param>
        public LocalDebugPathAttribute(string path)
        {
            Path = path;
        }
    }
}
