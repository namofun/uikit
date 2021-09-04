using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    /// <summary>
    /// The smoke test target of component version.
    /// </summary>
    public class ComponentVersion
    {
        /// <summary>
        /// The assembly name
        /// </summary>
        public string? AssemblyName { get; set; }

        /// <summary>
        /// The assembly version
        /// </summary>
        public string? Version { get; set; }

        /// <summary>
        /// The git branch
        /// </summary>
        public string? Branch { get; set; }

        /// <summary>
        /// The git commit ID
        /// </summary>
        public string? CommitId { get; set; }

        /// <summary>
        /// The signing public key
        /// </summary>
        public string? PublicKey { get; set; }

        /// <summary>
        /// Gets the version information from assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The component version.</returns>
        public static ComponentVersion FromAssembly(Assembly assembly)
        {
            GitVersionAttribute? gitVersion;

            try
            {
                gitVersion = assembly.GetCustomAttribute<GitVersionAttribute>();
            }
            catch
            {
                gitVersion = null;
            }

            var asName = assembly.GetName();

            return new ComponentVersion
            {
                AssemblyName = asName.Name,
                Branch = gitVersion?.Branch,
                CommitId = gitVersion?.Version,
                PublicKey = asName.GetPublicKeyToken()?.ToHexDigest(true),
                Version = asName.Version?.ToString(),
            };
        }
    }

    /// <summary>
    /// The system component list.
    /// </summary>
    public class SystemComponent
    {
        /// <summary>
        /// Whether current system enables Razor runtime compilation
        /// </summary>
        public bool RazorRuntimeCompilation { get; set; }

        /// <summary>
        /// All the loaded component versions
        /// </summary>
        public List<ComponentVersion> ComponentVersions { get; set; }

        /// <summary>
        /// Initialize the system component.
        /// </summary>
#pragma warning disable CS8618
        public SystemComponent()
#pragma warning restore CS8618
        {
        }
    }

    internal class ComponentSmokeTest : SmokeTestBase<SystemComponent>
    {
        private readonly IServiceProvider _serviceProvider;

        public ComponentSmokeTest(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override SystemComponent Get()
        {
            var comps = new List<ComponentVersion>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                comps.Add(ComponentVersion.FromAssembly(assembly));
            }

            return new SystemComponent
            {
                ComponentVersions = comps,
                RazorRuntimeCompilation = _serviceProvider.GetService(typeof(IRazorFileProvider)) != null,
            };
        }
    }
}
