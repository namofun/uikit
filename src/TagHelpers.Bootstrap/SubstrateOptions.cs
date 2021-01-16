using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Provide options for substrate.
    /// </summary>
    public class SubstrateOptions
    {
        /// <summary>
        /// Gets or sets the endpoint route builder configure actions.
        /// </summary>
        public List<Action<IEndpointRouteBuilder>> Endpoints { get; } = new List<Action<IEndpointRouteBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point 1 configure actions.
        /// </summary>
        public List<Action<IApplicationBuilder>> Point1 { get; } = new List<Action<IApplicationBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point 2 configure actions.
        /// </summary>
        public List<Action<IApplicationBuilder>> Point2 { get; } = new List<Action<IApplicationBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point 3 configure actions.
        /// </summary>
        public List<Action<IApplicationBuilder>> Point3 { get; } = new List<Action<IApplicationBuilder>>();
    }
}
