using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc
{
    public partial class Startup
    {
        /// <summary>
        /// Batch add the application parts and razor files into the <see cref="ApplicationPartManager"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure more.</param>
        public void ConfigureParts(IMvcBuilder builder)
        {
            var partList = new List<ApplicationPart>();
            PeerFileProvider? razorTree = null;

            static Assembly TryLoad(string assemblyName)
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.GetName().Name == assemblyName);
                if (assembly != null) return assembly;

                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory!, assemblyName + ".dll");
                if (!File.Exists(fullPath)) throw new TypeLoadException($"The assembly {assemblyName} is not found.");
                return Assembly.LoadFrom(fullPath);
            }

            void DiscoverPath(string localDebugPath, string areaName)
            {
                razorTree ??= new PeerFileProvider();
                string subpath;

                if (Directory.Exists(subpath = Path.Combine(localDebugPath, "Views")))
                {
                    var subtree = string.IsNullOrEmpty(areaName) ? razorTree : razorTree["Areas"][areaName];
                    subtree["Views"].Append(new PhysicalFileProvider(subpath));
                }

                if (string.IsNullOrEmpty(areaName) && Directory.Exists(subpath = Path.Combine(localDebugPath, "Areas")))
                {
                    razorTree["Areas"].Append(new PhysicalFileProvider(subpath));
                }

                if (Directory.Exists(subpath = Path.Combine(localDebugPath, "Panels")))
                {
                    razorTree["Areas"]["Dashboard"]["Views"].Append(new PhysicalFileProvider(subpath));
                }

                if (Directory.Exists(subpath = Path.Combine(localDebugPath, "Components")))
                {
                    razorTree["Views"]["Shared"]["Components"].Append(new PhysicalFileProvider(subpath));
                }
            }

            void DiscoverPart(Assembly assembly, string areaName)
            {
                var assemblyName = assembly.GetName().Name!;
                if (string.IsNullOrEmpty(assemblyName))
                {
                    throw new TypeLoadException("The assembly is invalid.");
                }

                if (!assemblyName.EndsWith(".Views"))
                {
                    partList.Add(new AssemblyPart(assembly));
                    var debugPath = assembly.GetCustomAttribute<LocalDebugPathAttribute>();
                    if (debugPath != null) DiscoverPath(debugPath.Path, areaName);
                }
                else if (assemblyName == "SatelliteSite.Substrate.Views")
                {
                    partList.Add(new CompiledRazorAssemblyPart(assembly));
                }
                else
                {
                    partList.Add(new ViewsAssemblyPart(assembly, areaName));
                }

                foreach (var related in assembly.GetCustomAttributes<RelatedAssemblyAttribute>())
                {
                    DiscoverPart(TryLoad(related.AssemblyFileName), areaName);
                }
            }

            foreach (var module in Modules)
            {
                DiscoverPart(module.GetType().Assembly, module.Area);
            }

            // Use reflection to discover things about runtime compilation
            //
            static void DiscoverRuntimeCompilation(IServiceCollection services, IRazorFileProvider? razorTree)
            {
                const string AssemblyName = "Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation";
                const string RuntimeViewCompilerProvider = AssemblyName + "." + nameof(RuntimeViewCompilerProvider);
                const string MvcRazorRuntimeCompilationOptions = AssemblyName + "." + nameof(MvcRazorRuntimeCompilationOptions);

                var compilerProvider = services.FirstOrDefault(f =>
                    f.ServiceType == typeof(IViewCompilerProvider) &&
                    f.ImplementationType?.Assembly?.GetName()?.Name == AssemblyName &&
                    f.ImplementationType?.FullName == RuntimeViewCompilerProvider);

                if (compilerProvider == null || razorTree == null) return;
                services.AddSingleton<IRazorFileProvider>(razorTree);
                var assembly = compilerProvider.ImplementationType.Assembly;
                var optionsType = assembly.GetType(MvcRazorRuntimeCompilationOptions)!;

                var options = Expression.Parameter(optionsType, "options");
                var lambda = Expression.Lambda(
                    Expression.Call(
                        Expression.Property(options, "FileProviders"),
                        typeof(ICollection<IFileProvider>).GetMethod(nameof(ICollection<IFileProvider>.Add)),
                        Expression.Constant(razorTree, typeof(IFileProvider))),
                    options);

                var action = lambda.Compile();
                var configureOptions = Activator.CreateInstance(typeof(ConfigureOptions<>).MakeGenericType(optionsType), action);
                services.ConfigureOptions(configureOptions);
            }

            DiscoverRuntimeCompilation(builder.Services, razorTree);

            foreach (var part in partList)
            {
                switch (part)
                {
                    case ViewsAssemblyPart vap:
                        var rcap = builder.PartManager.ApplicationParts
                            .OfType<CompiledRazorAssemblyPart>()
                            .SingleOrDefault(a => a.Assembly == vap.Assembly);
                        builder.PartManager.ApplicationParts.Remove(rcap);
                        builder.PartManager.ApplicationParts.Add(vap);
                        break;

                    case AssemblyPart ap:
                        if (!builder.PartManager.ApplicationParts
                            .OfType<AssemblyPart>()
                            .Any(a => a.Assembly == ap.Assembly))
                            builder.PartManager.ApplicationParts.Add(ap);
                        break;

                    case CompiledRazorAssemblyPart crap:
                        if (!builder.PartManager.ApplicationParts
                            .OfType<CompiledRazorAssemblyPart>()
                            .Any(a => a.Assembly == crap.Assembly))
                            builder.PartManager.ApplicationParts.Add(crap);
                        break;

                    default:
                        throw new NotImplementedException("Seems that HostBuilder-discovered shouldn't contain this one.");
                }
            }
        }
    }
}
