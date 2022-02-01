using System.Linq;

namespace Microsoft.Extensions.FileProviders
{
    internal class PermissionControl
    {
        public static bool IsAllowedDirectory(string[]? allowedRanges, string path)
        {
            path = "/" + path.Trim('/', '\\').Replace('\\', '/') + "/";
            return allowedRanges == null
                || allowedRanges.Any(range => path.StartsWith(range) || range.StartsWith(path));
        }

        public static string[]? GetAllowedSubdirectory(string[]? allowedRanges, string path)
        {
            path = "/" + path.Trim('/', '\\').Replace('\\', '/') + "/";

            if (allowedRanges == null
                || allowedRanges.Any(range => path.StartsWith(range)))
                return null;

            return allowedRanges
                .Where(range => range.StartsWith("/" + path + "/"))
                .Select(range => range.Substring(0, path.Length - 1))
                .ToArray();
        }

        public static bool IsAllowedFile(string[]? allowedRanges, string path)
        {
            path = "/" + path.TrimStart('/', '\\').Replace('\\', '/');
            return allowedRanges == null
                || allowedRanges.Any(range =>
                    path == range
                    || (path.StartsWith(range) && range.EndsWith('/'))
                );
        }
    }
}
