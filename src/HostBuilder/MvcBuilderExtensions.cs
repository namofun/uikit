using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Contains several static extension methods to help build <see cref="IMvcBuilder"/>.
    /// </summary>
    public static class SubstrateMvcBuilderExtensions
    {
        /// <summary>
        /// Add the <see cref="TimeSpanJsonConverter"/> into json serializer options.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/></param>
        /// <returns>The <see cref="IMvcBuilder"/></returns>
        public static IMvcBuilder AddTimeSpanJsonConverter(this IMvcBuilder builder)
        {
            return builder.AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new TimeSpanJsonConverter()));
        }

        /// <summary>
        /// Add the <see cref="SlugifyParameterTransformer"/> into route token transformer conventions.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/></param>
        /// <returns>The <see cref="IMvcBuilder"/></returns>
        public static IMvcBuilder UseSlugifyParameterTransformer(this IMvcBuilder builder)
        {
            builder.Services.Configure<MvcOptions>(options =>
                options.Conventions.Add(
                    new RouteTokenTransformerConvention(new SlugifyParameterTransformer())));
            return builder;
        }

        /// <summary>
        /// Replace the default <see cref="LinkGenerator"/> implemention with more routing token prob.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/></param>
        /// <returns>The <see cref="IMvcBuilder"/></returns>
        public static IMvcBuilder ReplaceDefaultLinkGenerator(this IMvcBuilder builder)
        {
            var old = builder.Services.FirstOrDefault(s => s.ServiceType == typeof(LinkGenerator));
            OrderLinkGenerator.typeInner = old.ImplementationType;
            builder.Services.Replace(ServiceDescriptor.Singleton<LinkGenerator, OrderLinkGenerator>());
            return builder;
        }

        /// <summary>
        /// Batch add the application parts and razor files into the <see cref="ApplicationPartManager"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IMvcBuilder"/> to configure more.</param>
        /// <param name="modules">The list of <see cref="AbstractModule"/>.</param>
        /// <param name="isDevelopment">Whether the current environment is development.</param>
        /// <returns>The <see cref="IMvcBuilder"/> to chain the conventions.</returns>
        public static IMvcBuilder AddOnboardingModules(this IMvcBuilder builder, IReadOnlyCollection<AbstractModule> modules, bool isDevelopment)
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
                        if (Directory.Exists(dir1))
                            tree["Areas"][areaName]["Views"].Append(new PhysicalFileProvider(dir1));
                        var dir2 = Path.Combine(rdpa.Path, "Panels");
                        if (Directory.Exists(dir2))
                            tree["Areas"]["Dashboard"]["Views"].Append(new PhysicalFileProvider(dir2));
                        var dir3 = Path.Combine(rdpa.Path, "Components");
                        if (Directory.Exists(dir3))
                            tree["Views"]["Shared"]["Components"].Append(new PhysicalFileProvider(dir3));
                    }
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
            if (selfCheck != null)
                (tree ??= new PeerFileProvider()).Append(new PhysicalFileProvider(selfCheck.Path));

            foreach (var module in modules)
                Add(module.GetType().Assembly, module.Area);

            if (isDevelopment && tree != null)
                builder.AddRazorRuntimeCompilation(options => options.FileProviders.Add(tree));

            builder.ConfigureApplicationPartManager(apm =>
            {
                foreach (var part in lst)
                {
                    switch (part)
                    {
                        case ViewsAssemblyPart vap:
                            var rcap = apm.ApplicationParts
                                .OfType<CompiledRazorAssemblyPart>()
                                .SingleOrDefault(a => a.Assembly == vap.Assembly);
                            if (rcap != null)
                                apm.ApplicationParts.Remove(rcap);
                            apm.ApplicationParts.Add(vap);
                            break;

                        case AssemblyPart ap:
                            var rap = apm.ApplicationParts
                                .OfType<AssemblyPart>()
                                .Any(a => a.Assembly == ap.Assembly);
                            if (!rap)
                                apm.ApplicationParts.Add(ap);
                            break;
                    }
                }
            });

            return builder;
        }
    }
}
