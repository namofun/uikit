﻿using Microsoft.AspNetCore.Builder;
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
        /// Gets or sets the site name.
        /// </summary>
        public string SiteName { get; set; } = "Satellite Site";

        /// <summary>
        /// Gets the substrate version
        /// </summary>
        public string Version { get; } = typeof(SubstrateOptions).Assembly?.GetName()?.Version?.ToString() ?? "0.0.0.0";

        /// <summary>
        /// Gets or sets the gravatar mirror. (like <c>//www.gravatar.com/avatar/</c>)
        /// </summary>
        public string GravatarMirror { get; set; } = "//www.gravatar.com/avatar/";

        /// <summary>
        /// The route name of logout action
        /// </summary>
        public string LogoutRouteName { get; set; } = string.Empty;

        /// <summary>
        /// The route name of login action
        /// </summary>
        public string LoginRouteName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint route builder configure actions.
        /// </summary>
        public List<Action<IEndpointRouteBuilder>> Endpoints { get; } = new List<Action<IEndpointRouteBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point before URL rewriting.
        /// </summary>
        public List<Action<IApplicationBuilder>> PointBeforeUrlRewriting { get; } = new List<Action<IApplicationBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point before routing.
        /// </summary>
        public List<Action<IApplicationBuilder>> PointBeforeRouting { get; } = new List<Action<IApplicationBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point before endpoint execution.
        /// </summary>
        public List<Action<IApplicationBuilder>> PointBeforeEndpoint { get; } = new List<Action<IApplicationBuilder>>();

        /// <summary>
        /// Gets or sets the application builder extension point between authentication and authorization.
        /// </summary>
        public List<Action<IApplicationBuilder>> PointBetweenAuth { get; } = new List<Action<IApplicationBuilder>>();
    }
}
