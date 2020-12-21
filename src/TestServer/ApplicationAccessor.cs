namespace SatelliteSite.Tests
{
    /// <summary>
    /// Provide access to current <see cref="SubstrateApplicationBase"/> instance.
    /// </summary>
    public interface IApplicationAccessor
    {
        /// <summary>
        /// The <see cref="SubstrateApplicationBase"/> instance
        /// </summary>
        SubstrateApplicationBase Instance { get; }
    }
}
