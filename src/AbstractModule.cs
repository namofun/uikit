using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

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
        public virtual void RegisterEndpoints(IEndpointRouteBuilder endpoints)
        {
        }

        /// <summary>
        /// Register the services with corresponding lifetimes.
        /// </summary>
        /// <param name="services">The dependency injection builder</param>
        public virtual void RegisterServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Claims that the status code page in this root should fallback to this endpoint.
        /// </summary>
        /// <param name="pattern">The url pattern</param>
        /// <param name="endpoint">The endpoint to fallback, or no status page if null</param>
        protected void ClaimsStatusCodePage(string pattern, RouteEndpoint? endpoint)
        {

        }
    }

    /// <summary>
    /// Abstract base class for modules to load.
    /// </summary>
    public abstract class AbstractModule<TContext> : AbstractModule
        where TContext : DbContext
    {
        /// <summary>
        /// The concrete <see cref="DbContext"/> type
        /// </summary>
        public Type DbContextType { get; } = typeof(TContext);

        /// <summary>
        /// Register the entities with <typeparamref name="TContext"/>.
        /// </summary>
        /// <param name="modelBuilder">The model builder</param>
        public virtual void RegisterEntities(ModelBuilder modelBuilder)
        {
        }
    }
}
