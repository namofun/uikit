using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Abstract base class for connectors to load.
    /// </summary>
    public abstract class AbstractConnector
    {
        /// <summary>
        /// The real area name
        /// </summary>
        public abstract string Area { get; }

        /// <summary>
        /// The affiliate to attribute
        /// </summary>
        internal AffiliateToAttribute AffiliateToAttribute { get; set; } = null!; // will be set when created

        /// <summary>
        /// The belonging module
        /// </summary>
        internal AbstractModule Module { get; set; } = null!; // will be set when created

        /// <summary>
        /// Register the menu.
        /// </summary>
        /// <param name="menus">The menu contributor</param>
        public virtual void RegisterMenu(IMenuContributor menus)
        {
        }

        /// <summary>
        /// Register the endpoints with some more configurations.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder</param>
        public virtual void RegisterEndpoints(IEndpointBuilder endpoints)
        {
        }

        /// <summary>
        /// Register the services with corresponding lifetimes.
        /// </summary>
        /// <param name="services">The dependency injection builder</param>
        public virtual void RegisterServices(IServiceCollection services)
        {
        }
    }
}
