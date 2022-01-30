using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Extensions.FileProviders.Physical
{
    public class PhysicalWwwrootProvider : PhysicalFileProvider, IWwwrootFileProvider
    {
        public PhysicalWwwrootProvider(string root) : base(root)
        {
        }

        public async Task<IFileInfo> WriteStreamAsync(string subpath, Stream content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            IFileInfo fileInfo = GetFileInfo(subpath);
            if (fileInfo is NotFoundFileInfo || fileInfo.IsDirectory)
            {
                throw new InvalidOperationException();
            }

            EnsureDirectoryExists(fileInfo.PhysicalPath);

            FileInfo fileInfo2 = new(fileInfo.PhysicalPath);
            using FileStream fs = fileInfo2.Open(FileMode.Create);
            await content.CopyToAsync(fs);
            return fileInfo;
        }

        private static void EnsureDirectoryExists(string subpath)
        {
            string path = Path.GetDirectoryName(subpath)!;
            Directory.CreateDirectory(path);
        }
    }
}
