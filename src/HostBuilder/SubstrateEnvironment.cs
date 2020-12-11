using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// Provides information about the substrate environment an application is running in.
    /// </summary>
    internal interface ISubstrateEnvironment : IWebHostEnvironment
    {
        /// <summary>
        /// The collection for all injected substrate modules
        /// </summary>
        IReadOnlyCollection<AbstractModule> Modules { get; }
    }

    /// <summary>
    /// The wrapper for <see cref="HostingEnvironment"/>
    /// </summary>
    internal class SubstrateEnvironment :
        ISubstrateEnvironment,
        IWebHostEnvironment, IHostEnvironment,
#pragma warning disable CS0618
        IHostingEnvironment, Extensions.Hosting.IHostingEnvironment
#pragma warning restore CS0618
    {
        /// <summary>
        /// Construct the wrapper environment.
        /// </summary>
        /// <param name="environment">The original environment.</param>
        /// <param name="modules">The total modules.</param>
        public SubstrateEnvironment(IWebHostEnvironment environment, IReadOnlyCollection<AbstractModule> modules)
        {
            WebRootFileProvider = environment.WebRootFileProvider;
            WebRootPath = environment.WebRootPath;
            EnvironmentName = environment.EnvironmentName;
            ApplicationName = environment.ApplicationName;
            ContentRootFileProvider = environment.ContentRootFileProvider;
            ContentRootPath = environment.ContentRootPath;
            Modules = modules;
        }

        /// <inheritdoc />
        public IFileProvider WebRootFileProvider { get; set; }

        /// <inheritdoc />
        public string WebRootPath { get; set; }

        /// <inheritdoc />
        public string EnvironmentName { get; set; }

        /// <inheritdoc />
        public string ApplicationName { get; set; }

        /// <inheritdoc />
        public string ContentRootPath { get; set; }

        /// <inheritdoc />
        public IFileProvider ContentRootFileProvider { get; set; }

        /// <inheritdoc />
        public IReadOnlyCollection<AbstractModule> Modules { get; }
    }
}
