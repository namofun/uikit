using Microsoft.AspNetCore.Razor.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    /// <summary>
    /// The <see cref="CompiledRazorAssemblyPart"/> with razor file probe.
    /// </summary>
    public class ViewsAssemblyPart : ApplicationPart, IRazorCompiledItemProvider
    {
        /// <summary>
        /// Create instance of <see cref="ViewsAssemblyPart"/>.
        /// </summary>
        /// <param name="assembly">The assembly</param>
        /// <param name="areaName">The default area name</param>
        public ViewsAssemblyPart(Assembly assembly, string areaName)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            AreaName = areaName;
        }

        /// <summary>
        /// The default area name
        /// </summary>
        public string AreaName { get; }

        /// <summary>
        /// The assembly
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// The assembly name
        /// </summary>
        public override string Name => Assembly.GetName().Name!;

        /// <summary>
        /// Get the compiled items
        /// </summary>
        public IEnumerable<RazorCompiledItem> CompiledItems
        {
            get
            {
                var loader = new Razor2CompiledItemLoader(AreaName);
                return loader.LoadItems(Assembly);
            }
        }
    }
}
