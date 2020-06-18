using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Hosting
{
    /// <inheritdoc />
    internal class Razor2CompiledItem : RazorCompiledItem
    {
        /// <inheritdoc />
        public override string Identifier { get; }

        /// <inheritdoc />
        public override string Kind { get; }

        /// <inheritdoc />
        public override IReadOnlyList<object> Metadata { get; }

        /// <inheritdoc />
        public override Type Type { get; }

        /// <summary>
        /// Probe the correct file name.
        /// </summary>
        /// <param name="fileName">The razor file identifier</param>
        /// <returns>The correct identifier</returns>
        private static string IdentifierProbing(string fileName, string areaName)
        {
            if (fileName.StartsWith("/Views"))
                return "/Areas/" + areaName + fileName;
            else if (fileName.StartsWith("/Panels/"))
                return "/Areas/Dashboard/" + fileName["/Panels".Length..];
            else
                throw new ArgumentException($"The file {fileName} violates the discovery rules.");
        }

        /// <summary>
        /// Create <see cref="RazorCompiledItem"/> with correct directory for area.
        /// </summary>
        /// <param name="attr">The <see cref="RazorCompiledItemAttribute"/></param>
        /// <param name="areaName">The area name</param>
        public Razor2CompiledItem(RazorCompiledItemAttribute attr, string areaName)
        {
            Type = attr.Type;
            Kind = attr.Kind;
            Identifier = IdentifierProbing(attr.Identifier, areaName);

            Metadata = Type.GetCustomAttributes(inherit: true).Select(o =>
                o is RazorSourceChecksumAttribute rsca
                    ? new RazorSourceChecksumAttribute(rsca.ChecksumAlgorithm, rsca.Checksum, IdentifierProbing(rsca.Identifier, areaName))
                    : o).ToList();
        }
    }
}
