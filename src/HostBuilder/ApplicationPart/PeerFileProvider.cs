using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders.Composite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Extensions.FileProviders
{
    /// <summary>
    /// The tree or composite peer file provider.
    /// </summary>
    public class PeerFileProvider : IRazorFileProvider, IDirectoryContents, IFileInfo
    {
        #region Useless Properties

        /// <summary>The logger to log things</summary>
        public ILogger Logger { get; private set; } = NullLogger.Instance;

        /// <inheritdoc />
        public bool Exists => true;

        /// <inheritdoc />
        bool IFileInfo.IsDirectory => true;

        /// <inheritdoc />
        long IFileInfo.Length => -1;

        /// <inheritdoc />
        string? IFileInfo.PhysicalPath => null;

        /// <inheritdoc />
        Stream IFileInfo.CreateReadStream() => throw new InvalidOperationException("Cannot create a stream for a directory.");

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public string? Name { get; private set; } = "/";

        #endregion

        internal static bool NotNulls(IFileInfo? fileInfo)
            => !(fileInfo == null || fileInfo is NotFoundFileInfo || !fileInfo.Exists);

        internal static bool NotNulls(IChangeToken? changeToken)
            => !(changeToken == null || changeToken is NullChangeToken);

        /// <summary> The internal tree </summary>
        private Dictionary<string, PeerFileProvider>? Tree { get; set; }

        /// <summary> The composite providers </summary>
        private List<PhysicalFileProvider>? Composite { get; set; }

        /// <inheritdoc />
        public DateTimeOffset LastModified { get; private set; } = DateTimeOffset.Now;

        /// <inheritdoc />
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            if (Tree == null && Composite == null)
            {
                return new NotFoundDirectoryContents();
            }

            return new CompositeDirectoryContents(
                Enumerable.Empty<IFileProvider>()
                    .ConcatIf(Tree != null, Tree?.Values)
                    .ConcatIf(Composite != null, Composite)
                    .ToArray(),
                subpath);
        }

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            if (Tree == null && Composite == null)
            {
                return Enumerable.Empty<IFileInfo>().GetEnumerator();
            }

            return Enumerable.Empty<IFileInfo>()
                .ConcatIf(Tree != null, Tree?.Values?.SelectMany(a => a))
                .ConcatIf(Composite != null, Composite?.SelectMany(a => a.GetDirectoryContents("/")))
                .GetEnumerator();
        }

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == "/")
            {
                return this;
            }

            if (subpath.StartsWith("/Pages/", StringComparison.OrdinalIgnoreCase))
            {
                return new NotFoundFileInfo(subpath);
            }

            var candidates = new List<IFileInfo>();
            if (Composite != null)
            {
                candidates.AddRange(Composite.Select(a => a.GetFileInfo(subpath)));
            }

            if (Tree != null)
            {
                ReadOnlySpan<char> ch = subpath;
                ch = ch.TrimStart('/');
                var idx = ch.IndexOf('/');

                if (idx != -1)
                {
                    var newSubpath = ch.Slice(idx).ToString();
                    var smallPath = ch.Slice(0, idx).ToString();
                    if (Tree.TryGetValue(smallPath, out var pfp))
                    {
                        candidates.Add(pfp.GetFileInfo(newSubpath));
                    }
                }
            }

            candidates = candidates.Where(NotNulls).ToList();
            if (candidates.Count == 0) return new NotFoundFileInfo(subpath);
            if (candidates.Count == 1) return candidates[0];

            Logger.LogError(
                "Multiple candidates while searching {subpath}. They are: \r\n{candidates}\r\nReturning the first one.",
                Name?.TrimEnd('/') + "/" + subpath.TrimStart('/'),
                string.Join("\r\n", candidates.Select(f => f.PhysicalPath ?? "(Unknown Physical Path)")));

            return candidates[0];
        }

        /// <inheritdoc />
        public IChangeToken Watch(string filter)
        {
            if (filter.StartsWith("/Pages/", StringComparison.OrdinalIgnoreCase))
            {
                return NullChangeToken.Singleton;
            }

            if (filter.StartsWith("/*", StringComparison.Ordinal))
            {
                var results = Enumerable.Empty<IChangeToken>()
                    .ConcatIf(Tree != null, Tree?.Values.Select(a => a.Watch(filter)))
                    .ConcatIf(Composite != null, Composite?.Select(a => a.Watch(filter)))
                    .Where(NotNulls)
                    .ToList();

                if (results.Count == 0) return NullChangeToken.Singleton;
                else if (results.Count == 1) return results[0];
                return new CompositeChangeToken(results);
            }
            else
            {
                var toks = new List<IChangeToken>();

                if (Composite != null)
                {
                    toks.AddRange(Composite.Select(a => a.Watch(filter)));
                }

                if (Tree != null)
                {
                    ReadOnlySpan<char> ch = filter;
                    ch = ch.TrimStart('/');
                    var idx = ch.IndexOf('/');

                    if (idx != -1)
                    {
                        var newSubpath = ch.Slice(idx).ToString();
                        var smallPath = ch.Slice(0, idx).ToString();
                        if (Tree.TryGetValue(smallPath, out var pfp))
                        {
                            toks.Add(pfp.Watch(newSubpath));
                        }
                    }
                }

                toks = toks.Where(NotNulls).ToList();
                if (toks.Count == 0) return NullChangeToken.Singleton;
                else if (toks.Count == 1) return toks[0];
                return new CompositeChangeToken(toks);
            }
        }

        /// <inheritdoc />
        public IRazorFileProvider this[string name]
        {
            get
            {
                Tree ??= new Dictionary<string, PeerFileProvider>();
                if (!Tree.TryGetValue(name, out var subp))
                {
                    Tree.Add(name, subp = new PeerFileProvider { Name = Name + name + "/" });
                }

                return subp;
            }
        }

        /// <inheritdoc />
        public IRazorFileProvider Append(PhysicalFileProvider fileProvider)
        {
            Composite ??= new List<PhysicalFileProvider>();
            Composite.Add(fileProvider);
            return this;
        }

        /// <inheritdoc />
        public void InjectLogger(ILogger? logger)
        {
            Logger = logger ?? NullLogger.Instance;
            if (Tree == null) return;
            foreach (var pfp in Tree.Values)
            {
                pfp.InjectLogger(logger);
            }
        }
    }
}
