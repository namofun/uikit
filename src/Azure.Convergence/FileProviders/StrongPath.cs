using System;
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
            if (subpath.Length == 0)
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

            if (subpath.EndsWith("/"))
            {
                throw new ArgumentException(
                    "Path cannot be a directory.",
                    nameof(subpath));
            }

            if (subpath.Contains("//"))
            {
                throw new ArgumentException(
                    "Path cannot include consecutive '/'.",
                    nameof(subpath));
            }

            if (InvalidCharacters.Any(subpath.Contains))
            {
                throw new ArgumentException(
                    "Path cannot include any characters in  '/'.",
                    nameof(subpath));
            }

            _path = subpath;
        }

        public string GetFileName()
        {
            return System.IO.Path.GetFileName(_path);
        }

        public string Normalize()
        {
            return _unusablePathChars.Replace(_path, "__");
        }

        public string GetLiteral()
        {
            return _path;
        }
    }
}
