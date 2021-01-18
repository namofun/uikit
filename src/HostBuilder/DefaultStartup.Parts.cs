using Microsoft.AspNetCore.Mvc.ApplicationParts;
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
    public partial class Startup
    {
        /// <summary>
        /// AssemblyLoadFileDelegate for <see cref="ApplicationParts.RelatedAssemblyAttribute"/>.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The loaded assembly.</returns>
        private static Assembly AssemblyLoadFileDelegate(string fileName)
        {
            bool AreSameAssembly(Assembly a) => !a.IsDynamic && string.Equals(a.Location, fileName, StringComparison.OrdinalIgnoreCase);

            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(AreSameAssembly);
            if (assembly != null) return assembly;
            return Assembly.LoadFile(fileName);
        }

        /// <summary>
        /// Setup the <see cref="AssemblyLoadFileDelegate"/>.
        /// </summary>
        static Startup()
        {
            // For ASP.NET Core 3.1
            typeof(ApplicationParts.RelatedAssemblyAttribute)
                .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(f => f.Name == nameof(AssemblyLoadFileDelegate))
                .SetValue(null, new Func<string, Assembly>(AssemblyLoadFileDelegate));
        }

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

            if (Environment.IsDevelopment() && razorTree != null)
            {
                builder.AddRazorRuntimeCompilation(options => options.FileProviders.Add(razorTree));
            }

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
