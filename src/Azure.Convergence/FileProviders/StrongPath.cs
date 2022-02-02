using Azure;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.FileProviders
{
    internal readonly struct StrongPath
    {
        private const string InvalidCharacters = ":?*\"'`#$&<>|%";
        private static readonly Regex _unusablePathChars =
            new("(" + string.Join('|', (InvalidCharacters + "/\\").Select(ch => Regex.Escape(ch.ToString()))) + ")");

        private readonly string _path;

        public StrongPath(string subpath)
        {
            subpath = subpath.TrimStart('/', '\\').Replace('\\', '/');

            EnsureValidPath_Length(subpath, includeZero: false);
            EnsureValidPath_NotDirectory(subpath);
            EnsureValidPath_Characters(subpath);

            _path = subpath;
        }

        public static void EnsureValidPath_Length(string subpath, bool includeZero)
        {
            if (!includeZero && subpath.Length == 0)
            {
                throw new ArgumentException(
                    "Path cannot be empty.",
                    nameof(subpath));
            }

            if (subpath.Length > 100)
            {
                throw new ArgumentException(
                    "Path cannot be longer than 100.",
                    nameof(subpath));
            }
        }

        public static void EnsureValidPath_NotDirectory(string subpath)
        {
            if (subpath.EndsWith("/"))
            {
                throw new ArgumentException(
                    "Path cannot be a directory.",
                    nameof(subpath));
            }
        }

        public static void EnsureValidPath_Characters(string subpath)
        {
            if (subpath.Contains("//"))
            {
                throw new ArgumentException(
                    "Path cannot include consecutive '/'.",
                    nameof(subpath));
            }

            if (subpath.Contains("/../"))
            {
                throw new ArgumentException(
                    "Path cannot include '/../'.",
                    nameof(subpath));
            }

            if (subpath.Contains("/./"))
            {
                throw new ArgumentException(
                    "Path cannot include '/./'.",
                    nameof(subpath));
            }

            if (InvalidCharacters.Any(subpath.Contains))
            {
                throw new ArgumentException(
                    "Path cannot include any characters in  '/'.",
                    nameof(subpath));
            }
        }

        public string GetFileName()
        {
            return Path.GetFileName(_path);
        }

        public string Normalize()
        {
            return _unusablePathChars.Replace(_path, "__");
        }

        public string GetLiteral()
        {
            return _path;
        }

        public string GetCachePath(string localCacheRoot, ETag etag)
        {
            return Path.Combine(localCacheRoot, Normalize() + "%" + etag);
        }
    }
}
