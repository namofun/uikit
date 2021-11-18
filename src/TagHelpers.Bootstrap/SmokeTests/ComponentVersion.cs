using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.Diagnostics.SmokeTests
{
    internal class ComponentSmokeTest : SmokeTestBase<SystemComponent>
    {
        private readonly IServiceProvider _serviceProvider;

        public ComponentSmokeTest(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private static ComponentVersion FromAssembly(Assembly assembly)
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

        protected override SystemComponent Get()
        {
            return new SystemComponent
            {
                ComponentVersions = AppDomain.CurrentDomain.GetAssemblies().Select(FromAssembly).ToList(),
                RazorRuntimeCompilation = _serviceProvider.GetService(typeof(IRazorFileProvider)) != null,
            };
        }
    }
}
