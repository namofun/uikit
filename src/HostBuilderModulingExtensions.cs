using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Provide several extension methods to add modules.
    /// </summary>
    public static class HostBuilderModulingExtensions
    {
        /// <summary>
        /// Add a module and configure them in the next constructing pipeline.
        /// </summary>
        /// <typeparam name="TModule">The only <see cref="AbstractModule"/> in that Assembly</typeparam>
        /// <param name="builder">The <see cref="IHostBuilder"/></param>
        /// <returns>The <see cref="IHostBuilder"/></returns>
        public static IHostBuilder AddModule<TModule>(this IHostBuilder builder) where TModule : AbstractModule, new()
        {
            Startup.Modules.Add(new TModule());
            var module = typeof(TModule);
            ApiExplorerVisibilityAttribute.DeclaredAssemblyModule.Add(module.Assembly.FullName!, module.FullName!);
            return builder;
        }

        /// <summary>
        /// Apply the endpoint modules into the route builder.
        /// </summary>
        /// <param name="builder">The route builder</param>
        /// <param name="modules">The endpoint configuration list</param>
        /// <returns>The route builder</returns>
        internal static void ApplyEndpoints(this ICollection<AbstractModule> modules, IEndpointRouteBuilder builder)
        {
            foreach (var module in modules)
            {
                module.RegisterEndpoints(ModuleEndpointDataSourceBase.Factory(module, builder));
            }
        }

        /// <summary>
        /// Apply the dependency into the dependency injection builder.
        /// </summary>
        /// <param name="builder">The dependency injection builder</param>
        /// <param name="modules">The dependency configuration list</param>
        internal static void ApplyServices(this ICollection<AbstractModule> modules, IServiceCollection builder)
        {
            foreach (var module in modules)
            {
                var type = typeof(ModuleEndpointDataSource<>).MakeGenericType(module.GetType());
                builder.AddSingleton(type);
                module.RegisterServices(builder);
            }
        }

        /// <summary>
        /// Apply the application parts into the manager.
        /// </summary>
        /// <param name="apm">The application part manager</param>
        /// <param name="modules">The application parts list</param>
        /// <returns>The application part manager</returns>
        internal static (List<ApplicationPart>, IFileProvider) GetParts(this ICollection<AbstractModule> modules)
        {
            var lst = new List<ApplicationPart>();

            static bool TryLoad(string assemblyName, out Assembly? assembly)
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + assemblyName))
                {
                    assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + assemblyName);
                    return true;
                }
                else
                {
                    assembly = null;
                    return false;
                }
            }

            void Add(Assembly assembly, string areaName)
            {
                var assemblyName = assembly.GetName().Name;
                if (string.IsNullOrEmpty(assemblyName))
                    throw new TypeLoadException("The assembly is invalid.");

                if (!assemblyName!.EndsWith(".Views"))
                {
                    lst.Add(new AssemblyPart(assembly));
                }
                else
                {
                    lst.Add(new ViewsAssemblyPart(assembly, areaName));
                }

                foreach (var rel in assembly.GetCustomAttributes<RelatedAssemblyAttribute>())
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == rel.AssemblyFileName).Any()) return;
                    if (!TryLoad(rel.AssemblyFileName + ".dll", out var ass))
                        throw new TypeLoadException("The assembly is invalid.");
                    Add(ass!, areaName);
                }
            }

            foreach (var module in modules)
                Add(module.GetType().Assembly, module.Area);

            return (lst, null!);
        }
    }
}
