using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    public class InMemoryFileProvider : IFileProvider, IBlobProvider
    {
        private readonly AsyncLock _directoryLocker;
        private readonly AsyncLock _fileLocker;
        private readonly Dictionary<string, InMemoryFile> _files;

        public InMemoryFileProvider()
        {
            _directoryLocker = new AsyncLock();
            _fileLocker = new AsyncLock();
            _files = new Dictionary<string, InMemoryFile>();
        }

        private class InMemoryFile : IFileInfo, IBlobInfo
        {
            private readonly string _subpath;
            private byte[] _content;
            private readonly AsyncLock _locker;

            public InMemoryFile(string subpath, AsyncLock lck)
            {
                _content = Array.Empty<byte>();
                _locker = lck;
                _subpath = subpath;
                LastModified = DateTimeOffset.Now;
            }

            public async Task CleanupAsync()
            {
                using var ___ = await _locker.LockAsync();
                _content = Array.Empty<byte>();
                LastModified = DateTimeOffset.Now;
            }

            public async Task WriteBinaryAsync(byte[] content)
            {
                using var ___ = await _locker.LockAsync();
                _content = content;
                LastModified = DateTimeOffset.Now;
            }

            public async Task WriteStreamAsync(Stream content)
            {
                using var ___ = await _locker.LockAsync();
                _content = new byte[content.Length];
                for (int i = 0; i < _content.Length; )
                    i += await content.ReadAsync(_content, i, _content.Length - i);
                LastModified = DateTimeOffset.Now;
            }

            public Task WriteStringAsync(string content)
            {
                var content2 = Encoding.UTF8.GetBytes(content);
                return WriteBinaryAsync(content2);
            }

            public string FullPath => _subpath;
            public bool Exists => true;
            public long Length => _content.Length;
            public string? PhysicalPath => null;
            public string Name => Path.GetFileName(_subpath);
            public DateTimeOffset LastModified { get; set; }
            public bool IsDirectory => false;
            public bool HasDirectLink => false;
            public Stream CreateReadStream() => new MemoryStream(_content, false);
            public Task<Stream> CreateReadStreamAsync(bool? cached) => Task.FromResult(CreateReadStream());
            public Task<Uri> CreateDirectLinkAsync(
                TimeSpan validPeriod,
                string? desiredDownloadName = null,
                string? desiredContentType = null,
                string? correlationId = null)
                => throw new NotSupportedException();
        }

        private class InMemoryDirectory : IFileInfo, IDirectoryContents, IBlobInfo
        {
            private readonly string _subpath;
            private readonly Lazy<IReadOnlyList<IFileInfo>> _kvps;

            public InMemoryDirectory(string subpath, IReadOnlyDictionary<string, InMemoryFile> files)
            {
                _subpath = subpath;
                LastModified = DateTimeOffset.Now;

                _kvps = new Lazy<IReadOnlyList<IFileInfo>>(() =>
                {
                    var query =
                    from kk in files.Keys
                    let k = kk[subpath.Length..]
                    let split = k.Split('/', 2)
                    group new { Key = kk, Value = files[kk] } by new { Segment = split[0], split.Length } into g
                    select g.Key.Length == 1
                        ? (IFileInfo)g.Single().Value
                        : new InMemoryDirectory(subpath + g.Key.Segment + "/", g.ToDictionary(a => a.Key, a => a.Value));
                    return query.ToList();
                });
            }

            public bool Exists => true;
            public long Length => -1;
            public string? PhysicalPath => null;
            public string Name => Path.GetFileName(_subpath.TrimEnd('/'));
            public DateTimeOffset LastModified { get; }
            public bool IsDirectory => true;
            public bool HasDirectLink => false;
            public Stream CreateReadStream() => throw new InvalidOperationException();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<IFileInfo> GetEnumerator() => _kvps.Value.GetEnumerator();
            public Task<Stream> CreateReadStreamAsync(bool? cached) => throw new InvalidOperationException();
            public Task<Uri> CreateDirectLinkAsync(
                TimeSpan validPeriod,
                string? desiredDownloadName = null,
                string? desiredContentType = null,
                string? correlationId = null)
                => throw new NotSupportedException();
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return (IFileInfo)GetFileInfoAsync(subpath).Result;
        }

        public async Task<IBlobInfo> GetFileInfoAsync(string subpath)
        {
            subpath = subpath.Replace('\\', '/').TrimStart('/');
            var subpath2 = subpath.TrimEnd('/') + "/";
            using var ___ = await _directoryLocker.LockAsync();

            if (_files.ContainsKey(subpath))
            {
                return _files[subpath];
            }

            if (subpath.EndsWith('/') || // "some/dir/"
                subpath == string.Empty || // "" -> "/"
                _files.Keys.Any(k => k.StartsWith(subpath2))) // exist "some/filename/xxx"
            {
                subpath = subpath2;
                if (subpath == "/") subpath = "";
                var _directoryFiles = _files
                    .Where(k => k.Key.StartsWith(subpath))
                    .ToDictionary(k => k.Key, v => v.Value);

                if (_directoryFiles.Count == 0)
                    return new NotFoundBlobInfo(subpath);
                return new InMemoryDirectory(subpath, _directoryFiles);
            }

            return new NotFoundBlobInfo(subpath);
        }

        public async Task<bool> RemoveFileAsync(string subpath)
        {
            subpath = subpath.Replace('\\', '/').TrimStart('/');
            if (subpath.EndsWith('/') || subpath == string.Empty)
                throw new ArgumentException("subpath should be file name!");

            InMemoryFile file;
            using (await _directoryLocker.LockAsync())
            {
                if (!_files.ContainsKey(subpath)) return false;
                file = _files[subpath];
                _files.Remove(subpath);
            }

            await file.CleanupAsync();
            return true;
        }

        private async Task<IBlobInfo> WriteFileAsync(string subpath, Func<InMemoryFile, Task> runner)
        {
            subpath = subpath.Replace('\\', '/').TrimStart('/');
            if (subpath.EndsWith('/') || subpath == string.Empty)
                throw new ArgumentException("subpath should be file name!");

            InMemoryFile fileInfo;
            using (await _directoryLocker.LockAsync())
            {
                if (_files.Keys.Any(k => k.StartsWith(subpath + "/")))
                {
                    throw new ArgumentException("directory exists!");
                }

                if (!_files.ContainsKey(subpath))
                {
                    _files.Add(subpath, new InMemoryFile(subpath, _fileLocker));
                }

                fileInfo = _files[subpath];
            }

            await runner.Invoke(fileInfo);
            return fileInfo;
        }

        public Task<IBlobInfo> WriteBinaryAsync(string subpath, byte[] content, string mime)
            => WriteFileAsync(subpath, f => f.WriteBinaryAsync(content));

        public Task<IBlobInfo> WriteStreamAsync(string subpath, Stream content, string mime)
            => WriteFileAsync(subpath, f => f.WriteStreamAsync(content));

        public Task<IBlobInfo> WriteStringAsync(string subpath, string content, string mime)
            => WriteFileAsync(subpath, f => f.WriteStringAsync(content));

        public IDirectoryContents GetDirectoryContents(string subpath)
            => GetFileInfo(subpath) as IDirectoryContents ?? new NotFoundDirectoryContents();

        public IChangeToken Watch(string filter)
            => NullChangeToken.Singleton;
    }
}
