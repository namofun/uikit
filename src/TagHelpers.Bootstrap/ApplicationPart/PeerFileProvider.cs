using Microsoft.AspNetCore.Mvc;
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
    public class PeerFileProvider : IFileProvider, IDirectoryContents, IFileInfo
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
                return new NotFoundDirectoryContents();
            var fps = Enumerable.Empty<IFileProvider>();
            if (Tree != null) fps = fps.Concat(Tree.Values);
            if (Composite != null) fps = fps.Concat(Composite);
            return new CompositeDirectoryContents(fps.ToArray(), subpath);
        }

        /// <inheritdoc />
        public IEnumerator<IFileInfo> GetEnumerator()
        {
            if (Tree == null && Composite == null)
                return Enumerable.Empty<IFileInfo>().GetEnumerator();
            var fps = Enumerable.Empty<IFileInfo>();
            if (Tree != null) fps = fps.Concat(Tree.Values.SelectMany(a => a));
            if (Composite != null) fps = fps.Concat(Composite.SelectMany(a => a.GetDirectoryContents("/")));
            return fps.GetEnumerator();
        }

        /// <inheritdoc />
        public IFileInfo GetFileInfo(string subpath)
        {
            if (subpath == "/") return this;
            if (subpath.StartsWith("/Pages/", StringComparison.OrdinalIgnoreCase))
                return new NotFoundFileInfo(subpath);

            var f1 = Composite?.Select(a => a.GetFileInfo(subpath)?.NullIfNotFound())
                .Where(a => a != null).SingleOrDefault();

            if (f1 == null && Tree != null)
            {
                ReadOnlySpan<char> ch = subpath;
                ch = ch.TrimStart('/');
                var idx = ch.IndexOf('/');

                if (idx != -1)
                {
                    var newSubpath = ch.Slice(idx).ToString();
                    var smallPath = ch.Slice(0, idx).ToString();
                    f1 = Tree?.GetValueOrDefault(smallPath)?.GetFileInfo(newSubpath)?.NullIfNotFound();
                }
            }

            return f1 ?? new NotFoundFileInfo(subpath);
        }

        /// <inheritdoc />
        public IChangeToken Watch(string filter)
        {
            if (filter.StartsWith("/Pages/", StringComparison.OrdinalIgnoreCase))
                return NullChangeToken.Singleton;

            if (filter.StartsWith("/*", StringComparison.Ordinal))
            {
                var toks = Enumerable.Empty<IChangeToken>();
                if (Tree != null) toks = toks.Concat(
                    Tree.Values.Select(a => a.Watch(filter).NullIfNotFound()!).Where(a => a != null));
                if (Composite != null) toks = toks.Concat(
                    Composite.Select(a => a.Watch(filter).NullIfNotFound()!).Where(a => a != null));
                var results = toks.ToArray();
                if (results.Length == 0) return NullChangeToken.Singleton;
                else if (results.Length == 1) return results[0];
                return new CompositeChangeToken(results);
            }
            else
            {
                var toks = Enumerable.Empty<IChangeToken?>();
                if (Composite != null) toks = toks.Concat(
                    Composite.Select(a => a.Watch(filter)));
                ReadOnlySpan<char> ch = filter;
                ch = ch.TrimStart('/');
                var idx = ch.IndexOf('/');

                if (idx != -1)
                {
                    var newSubpath = ch.Slice(idx).ToString();
                    var smallPath = ch.Slice(0, idx).ToString();
                    if (Tree != null) toks = toks.Append(
                        Tree.GetValueOrDefault(smallPath)?.Watch(newSubpath)?.NullIfNotFound());
                }

                var toks3 = toks.Where(a => a?.NullIfNotFound() != null).ToArray();
                if (toks3.Length == 0) return NullChangeToken.Singleton;
                else if (toks3.Length == 1) return toks3[0]!;
                return new CompositeChangeToken(toks3);
            }
        }

        /// <summary>
        /// Gets the sub file provider.
        /// </summary>
        /// <param name="name">The folder name.</param>
        /// <returns>The file provider.</returns>
        public PeerFileProvider this[string name]
        {
            get
            {
                if (Tree == null)
                    Tree = new Dictionary<string, PeerFileProvider>();
                if (!Tree.TryGetValue(name, out var subp))
                    Tree.Add(name, subp = new PeerFileProvider { Name = Name + name + "/" });
                return subp;
            }
        }

        /// <summary>
        /// Add the physical provider to this peer file provider.
        /// </summary>
        /// <param name="dest">The physical file provider.</param>
        /// <returns>The peer file provider.</returns>
        public PeerFileProvider Append(PhysicalFileProvider dest)
        {
            Composite ??= new List<PhysicalFileProvider>();
            Composite.Add(dest);
            return this;
        }
    }
}
