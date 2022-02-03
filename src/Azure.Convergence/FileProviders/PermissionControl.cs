using System.Linq;

namespace Microsoft.Extensions.FileProviders
{
    internal class PermissionControl
    {
        public static bool IsAllowedDirectory(string[]? allowedRanges, string path)
        {
            path = "/" + path.Trim('/', '\\').Replace('\\', '/') + "/";
            if (path == "//") path = "/";
            return allowedRanges == null
                || allowedRanges.Any(range => path.StartsWith(range) || range.StartsWith(path));
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
