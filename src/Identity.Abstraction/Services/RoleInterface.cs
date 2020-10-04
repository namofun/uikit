namespace SatelliteSite.IdentityModule.Services
{
    public interface IRole
    {
        /// <summary>
        /// Gets the primary key for this role.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the short name of the role.
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        /// Gets or sets the description of the role.
        /// </summary>
        string Description { get; set; }
    }
}
