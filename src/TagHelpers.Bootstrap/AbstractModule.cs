using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Abstract base class for modules to load.
    /// </summary>
    public abstract class AbstractModule
    {
        /// <summary>
        /// The real area name
        /// </summary>
        public abstract string Area { get; }

        /// <summary>
        /// Initialize this module, add some claims and conventions by calling protected class methods.
        /// </summary>
        public abstract void Initialize();

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
        /// <param name="configuration">The configuration</param>
        public virtual void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            RegisterServices(services);
        }

        /// <summary>
        /// Register the services with corresponding lifetimes.
        /// </summary>
        /// <param name="services">The dependency injection builder</param>
        public virtual void RegisterServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Register the menu.
        /// </summary>
        /// <param name="menus">The menu contributor</param>
        public virtual void RegisterMenu(IMenuContributor menus)
        {
        }
    }
}
