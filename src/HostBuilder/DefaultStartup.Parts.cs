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
            var lst = new List<ApplicationPart>();
            PeerFileProvider? tree = null;

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
                    var rdpa = assembly.GetCustomAttribute<LocalDebugPathAttribute>();
                    if (rdpa != null)
                    {
                        tree ??= new PeerFileProvider();
                        var dir1 = Path.Combine(rdpa.Path, "Views");
                        if (Directory.Exists(dir1) && !string.IsNullOrEmpty(areaName))
                            tree["Areas"][areaName]["Views"].Append(new PhysicalFileProvider(dir1));
                        else if (Directory.Exists(dir1) && string.IsNullOrEmpty(areaName))
                            tree["Views"].Append(new PhysicalFileProvider(dir1));
                        var dir2 = Path.Combine(rdpa.Path, "Panels");
                        if (Directory.Exists(dir2))
                            tree["Areas"]["Dashboard"]["Views"].Append(new PhysicalFileProvider(dir2));
                        var dir3 = Path.Combine(rdpa.Path, "Components");
                        if (Directory.Exists(dir3))
                            tree["Views"]["Shared"]["Components"].Append(new PhysicalFileProvider(dir3));
                    }
                }
                else if (assemblyName == "SatelliteSite.Substrate.Views")
                {
                    lst.Add(new CompiledRazorAssemblyPart(assembly));
                }
                else
                {
                    lst.Add(new ViewsAssemblyPart(assembly, areaName));
                }

                foreach (var rel in assembly.GetCustomAttributes<RelatedAssemblyAttribute>())
                {
                    Assembly? ass;
                    ass = AppDomain.CurrentDomain.GetAssemblies()
                        .SingleOrDefault(a => a.GetName().Name == rel.AssemblyFileName);
                    if (ass == null && !TryLoad(rel.AssemblyFileName + ".dll", out ass))
                        throw new TypeLoadException("The assembly is invalid.");
                    Add(ass!, areaName);
                }
            }

            var selfCheck = typeof(AbstractModule).Assembly
                .GetCustomAttribute<LocalDebugPathAttribute>();
            if (selfCheck != null && Directory.Exists(selfCheck.Path))
                (tree ??= new PeerFileProvider()).Append(new PhysicalFileProvider(selfCheck.Path));

            foreach (var module in Modules)
                Add(module.GetType().Assembly, module.Area);

            if (Environment.IsDevelopment() && tree != null)
                builder.AddRazorRuntimeCompilation(options => options.FileProviders.Add(tree));

            builder.ConfigureApplicationPartManager(apm =>
            {
                foreach (var part in lst)
                {
                    CompiledRazorAssemblyPart rcap;
                    AssemblyPart rap;

                    switch (part)
                    {
                        case ViewsAssemblyPart vap:
                            rcap = apm.ApplicationParts
                                .OfType<CompiledRazorAssemblyPart>()
                                .SingleOrDefault(a => a.Assembly == vap.Assembly);
                            if (rcap != null)
                                apm.ApplicationParts.Remove(rcap);
                            apm.ApplicationParts.Add(vap);
                            break;

                        case AssemblyPart ap:
                            rap = apm.ApplicationParts
                                .OfType<AssemblyPart>()
                                .FirstOrDefault(a => a.Assembly == ap.Assembly);
                            if (rap == null)
                                apm.ApplicationParts.Add(ap);
                            break;

                        case CompiledRazorAssemblyPart crap:
                            rcap = apm.ApplicationParts
                                .OfType<CompiledRazorAssemblyPart>()
                                .SingleOrDefault(a => a.Assembly == crap.Assembly);
                            if (rcap == null)
                                apm.ApplicationParts.Add(crap);
                            break;

                        default:
                            throw new NotImplementedException("Seems that HostBuilder-discovered shouldn't contain this one.");
                    }
                }
            });
        }
    }
}
