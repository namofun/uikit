namespace System
{
    /// <summary>
    /// Generate GUID.
    /// </summary>
    public class GuidGenerator
    {
        /// <summary>
        /// Creates a GUID.
        /// </summary>
        /// <returns>The created GUID.</returns>
        public virtual Guid Create() => Guid.NewGuid();
    }
}
