using System;

namespace Microsoft.AspNetCore.Razor.Hosting
{
    /// <inheritdoc />
    internal class Razor2CompiledItemLoader : RazorCompiledItemLoader
    {
        /// <summary>
        /// The area name
        /// </summary>
        public string AreaName { get; }

        /// <summary>
        /// Create instance of <see cref="Razor2CompiledItemLoader"/>.
        /// </summary>
        /// <param name="areaName">The area name</param>
        public Razor2CompiledItemLoader(string areaName)
        {
            AreaName = areaName;
        }

        /// <inheritdoc />
        protected override RazorCompiledItem CreateItem(RazorCompiledItemAttribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            return new Razor2CompiledItem(attribute, AreaName);
        }
    }
}
