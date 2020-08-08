using System;
using System.Collections.Generic;

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
            if (fileName.StartsWith("/Panels/"))
                return "/Areas/Dashboard/Views" + fileName["/Panels".Length..];
            else if (fileName.StartsWith("/Views/") && areaName == null)
                return fileName;
            else if (fileName.StartsWith("/Views/"))
                return "/Areas/" + areaName + fileName;
            else if (fileName.StartsWith("/Components/"))
                return "/Views/Shared" + fileName;
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
            var metadata = new List<object>();

            Type = attr.Type;
            Kind = attr.Kind;
            Identifier = IdentifierProbing(attr.Identifier, areaName);
            Metadata = metadata;

            foreach (var item in Type.GetCustomAttributes(inherit: true))
            {
                if (item is RazorSourceChecksumAttribute rsca)
                {
                    if (rsca.Identifier == "/_ViewImports.cshtml") continue;
                    if (rsca.Identifier == "/Panels/_ViewImports.cshtml") continue;
                    var newIdentifier = IdentifierProbing(rsca.Identifier, areaName);
                    metadata.Add(new RazorSourceChecksumAttribute(rsca.ChecksumAlgorithm, rsca.Checksum, newIdentifier));
                }
                else
                {
                    metadata.Add(item);
                }
            }
        }
    }
}
