﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        /// The default convention for all endpoints.
        /// </summary>
        internal System.Action<IEndpointConventionBuilder> Conventions { get; set; } = _ => { };

        /// <summary>
        /// The real area name
        /// </summary>
        public abstract string Area { get; }

        /// <summary>
        /// Initialize this module, add some claims and conventions by calling protected class methods.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Whether the module gives Identity support
        /// </summary>
        public virtual bool ProvideIdentity => false;

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
        /// <param name="environment">The environment</param>
        public virtual void RegisterServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            RegisterServices(services, configuration);
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
