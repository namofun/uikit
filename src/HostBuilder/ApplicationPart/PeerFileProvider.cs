using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders.Composite;
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

        /// <summary>
        /// Filter out the <see cref="NotFoundFileInfo"/>.
        /// </summary>
        /// <param name="fileInfo">The source <see cref="IFileInfo"/>.</param>
        /// <returns>The <see cref="IFileInfo"/>.</returns>
        internal static IFileInfo? NullIfNotFound(IFileInfo? fileInfo)
        {
            if (fileInfo == null || fileInfo is NotFoundFileInfo || !fileInfo.Exists) return null;
            return fileInfo;
        }

        /// <summary>
        /// Filter out the <see cref="NullChangeToken"/>.
        /// </summary>
        /// <param name="changeToken">The source <see cref="IChangeToken"/>.</param>
        /// <returns>The <see cref="IChangeToken"/>.</returns>
        internal static IChangeToken? NullIfNotFound(IChangeToken? changeToken)
        {
            if (changeToken == null || changeToken is NullChangeToken) return null;
            return changeToken;
        }

        internal static bool NotNulls(IFileInfo? fileInfo)
        {
            return NullIfNotFound(fileInfo) != null;
        }

        internal static bool NotNulls(IChangeToken? changeToken)
        {
            return NullIfNotFound(changeToken) != null;
        }

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

            var f1 = Composite?
                .Select(a => a.GetFileInfo(subpath))
                .Where(NotNulls)
                .SingleOrDefault();

            if (f1 == null && Tree != null)
            {
                ReadOnlySpan<char> ch = subpath;
                ch = ch.TrimStart('/');
                var idx = ch.IndexOf('/');

                if (idx != -1)
                {
                    var newSubpath = ch.Slice(idx).ToString();
                    var smallPath = ch.Slice(0, idx).ToString();
                    f1 = NullIfNotFound(Tree?.GetValueOrDefault(smallPath)?.GetFileInfo(newSubpath));
                }
            }

            return f1 ?? new NotFoundFileInfo(subpath);
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
                    .ToArray();

                if (results.Length == 0) return NullChangeToken.Singleton;
                else if (results.Length == 1) return results[0];
                return new CompositeChangeToken(results);
            }
            else
            {
                var toks = new List<IChangeToken>();
                if (Composite != null)
                {
                    toks.AddRange(Composite.Select(a => a.Watch(filter)));
                }

                ReadOnlySpan<char> ch = filter;
                ch = ch.TrimStart('/');
                var idx = ch.IndexOf('/');

                if (idx != -1)
                {
                    var newSubpath = ch.Slice(idx).ToString();
                    var smallPath = ch.Slice(0, idx).ToString();
                    if (Tree != null)
                    {
                        toks.Add(Tree.GetValueOrDefault(smallPath)!.Watch(newSubpath));
                    }
                }

                var toks3 = toks.Where(NotNulls).ToArray();
                if (toks3.Length == 0) return NullChangeToken.Singleton;
                else if (toks3.Length == 1) return toks3[0]!;
                return new CompositeChangeToken(toks3);
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
    }
}
