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
