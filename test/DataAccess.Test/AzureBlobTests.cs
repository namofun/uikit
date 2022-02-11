using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class AzureBlobTests
    {
        private BlobContainerClient blobClient;

        private AzureBlobProvider Create(bool allowAutoCache = false, string[] allowedRanges = null)
        {
            return new(
                blobClient,
                Path.GetFullPath("./BlobContainerTestRoot"),
                default,
                allowAutoCache,
                allowedRanges);
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            blobClient = new BlobContainerClient("UseDevelopmentStorage=true", "localtest");
            await TestCleanup();

            await blobClient.CreateAsync();
            Directory.CreateDirectory("./BlobContainerTestRoot");
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await blobClient.DeleteIfExistsAsync();
            if (Directory.Exists("./BlobContainerTestRoot"))
            {
                Directory.Delete("./BlobContainerTestRoot", true);
            }
        }

        [TestMethod]
        public async Task WriteString()
        {
            IBlobProvider blobProvider = Create();
            IBlobInfo file = await blobProvider.WriteStringAsync("/WriteString.txt", "hello, world!", "text/plain");

            Assert.AreEqual("WriteString.txt", file.Name);
            Assert.AreEqual(13, file.Length);

            BlobClient client = blobClient.GetBlobClient("/WriteString.txt");
            Assert.IsTrue(await client.ExistsAsync());

            BlobProperties properties = await client.GetPropertiesAsync();
            Assert.AreEqual(13, properties.ContentLength);
            Assert.AreEqual("text/plain", properties.ContentType);
        }

        [TestMethod]
        public async Task WriteStream()
        {
            IWritableFileProvider blobProvider = Create(true);
            byte[] content = Guid.Parse("78c0fbfd-ca16-499e-aa4a-82ecb63d5d3c").ToByteArray();
            IFileInfo file = await blobProvider.WriteStreamAsync("/WriteStream.json", new MemoryStream(content));

            Assert.AreEqual("WriteStream.json", file.Name);
            Assert.AreEqual(16, file.Length);

            BlobClient client = blobClient.GetBlobClient("/WriteStream.json");
            Assert.IsTrue(await client.ExistsAsync());

            BlobProperties properties = await client.GetPropertiesAsync();
            Assert.AreEqual(16, properties.ContentLength);
            Assert.AreEqual("application/json", properties.ContentType);

            IFileInfo fileInfo = blobProvider.GetFileInfo("/WriteStream.json");
            Assert.IsNull(fileInfo.PhysicalPath);

            using (file.CreateReadStream()) { }
            Assert.IsNotNull(fileInfo.PhysicalPath);
            Assert.IsTrue(File.Exists(fileInfo.PhysicalPath));
        }

        [TestMethod]
        public async Task GetFileInfo_AutoCache()
        {
            byte[] content = Guid.Parse("78c0fbfd-ca16-499e-aa4a-82ecb63d5d3c").ToByteArray();

            BlobContentInfo info = await blobClient.GetBlobClient("/GetFileInfo_AutoCache.bin").UploadAsync(
                new MemoryStream(content),
                new BlobUploadOptions()
                {
                    Metadata = new Dictionary<string, string>()
                    {
                        ["LocalCacheGuid"] = "@fvAeBbKnkmqSoLstj1dPA",
                    },
                    HttpHeaders = new BlobHttpHeaders()
                    {
                        ContentType = "application/octet-stream",
                    },
                });

            IBlobProvider blobProvider = Create(false);
            IBlobInfo blobInfo = await blobProvider.GetFileInfoAsync("/GetFileInfo_AutoCache.bin");
            Assert.AreEqual(16, blobInfo.Length);
            Assert.AreEqual("GetFileInfo_AutoCache.bin", blobInfo.Name);

            string cacheFile = Path.Combine(
                Path.GetFullPath("./BlobContainerTestRoot"),
                "GetFileInfo_AutoCache.bin%" + info.ETag);
            Assert.IsFalse(File.Exists(cacheFile));

            using (Stream stream = await blobInfo.CreateReadStreamAsync(null))
            {
                byte[] cache = new byte[16];
                await stream.ReadAsync(cache.AsMemory());

                Assert.IsTrue(content.SequenceEqual(cache));
                Assert.IsFalse(File.Exists(cacheFile));
                Assert.IsNull(blobInfo.PhysicalPath);
            }

            using (Stream stream = await blobInfo.CreateReadStreamAsync(false))
            {
                byte[] cache = new byte[16];
                await stream.ReadAsync(cache.AsMemory());

                Assert.IsTrue(content.SequenceEqual(cache));
                Assert.IsFalse(File.Exists(cacheFile));
                Assert.IsNull(blobInfo.PhysicalPath);
            }

            using (Stream stream = await blobInfo.CreateReadStreamAsync(true))
            {
                byte[] cache = new byte[16];
                await stream.ReadAsync(cache.AsMemory());

                Assert.IsTrue(content.SequenceEqual(cache));
                Assert.IsTrue(File.Exists(cacheFile));
                Assert.AreEqual(blobInfo.PhysicalPath, cacheFile);
            }

            byte[] corruptContent = Guid.Parse("87c0fbfd-ca16-499e-aa4a-82ecb63d5d3c").ToByteArray();
            await File.WriteAllBytesAsync(blobInfo.PhysicalPath, corruptContent);

            using (Stream stream = await blobInfo.CreateReadStreamAsync(true))
            {
                byte[] cache = new byte[16];
                await stream.ReadAsync(cache.AsMemory());

                Assert.IsTrue(corruptContent.SequenceEqual(cache));
                Assert.IsTrue(File.Exists(cacheFile));
                Assert.AreEqual(blobInfo.PhysicalPath, cacheFile);
            }

            using (Stream stream = await blobInfo.CreateReadStreamAsync(false))
            {
                byte[] cache = new byte[16];
                await stream.ReadAsync(cache.AsMemory());

                Assert.IsTrue(content.SequenceEqual(cache));
                Assert.IsTrue(File.Exists(cacheFile));
                Assert.AreEqual(blobInfo.PhysicalPath, cacheFile);
            }
        }

        [TestMethod]
        public async Task DirectLink()
        {
            IBlobProvider blobProvider = Create();
            IBlobInfo file = await blobProvider.WriteStringAsync("/DirectLink.txt", "hello, dl!", "text/plain");

            Assert.IsTrue(file.HasDirectLink);

            Uri url = await file.CreateDirectLinkAsync(TimeSpan.FromMinutes(1), "my.txt", "application/octet-stream", "aaaabbbbccccdddd");
            using HttpClient httpClient = new();
            Assert.AreEqual("hello, dl!", await httpClient.GetStringAsync(url));
        }

        [TestMethod]
        public async Task DirectoryContents()
        {
            await ClearBlobFiles();

            Dictionary<string, (byte[] Content, byte[] Hash)> customBlobs = new();
            string[] blobNames = new[]
            {
                "FolderA/1x.json",
                "FolderA/2x.json",
                "FolderA/1x/json",
                "root.json",
                "FolderA/1x/json2",
                "FolderB/2f.json",
                "FolderA/1y/2f.json",
            };

            for (int i = 0; i < blobNames.Length; i++)
            {
                int contentLength = RandomNumberGenerator.GetInt32(100, 400);
                byte[] content = RandomNumberGenerator.GetBytes(contentLength);
                byte[] hash = content.ToMD5();

                customBlobs[blobNames[i]] = (content, hash);
                await blobClient.UploadBlobAsync(blobNames[i], BinaryData.FromBytes(content));
            }

            IFileProvider fileProvider = Create();
            void AssertDirectory(string path, string[] fileNames)
                => AssertDirectoryContents(
                    fileProvider.GetDirectoryContents(path),
                    fileNames,
                    (file, fileName) => Assert.AreEqual(customBlobs[fileName].Content.Length, file.Length));

            AssertDirectory("FolderA/1x/", new[]
            {
                "FolderA/1x/json",
                "FolderA/1x/json2",
            });

            AssertDirectory("/FolderA/", new[]
            {
                "FolderA/1x/",
                "FolderA/1y/",
                "FolderA/1x.json",
                "FolderA/2x.json",
            });

            AssertDirectory("/", new[]
            {
                "FolderA/",
                "FolderB/",
                "root.json",
            });
        }

        [TestMethod]
        public async Task PermissionControl()
        {
            await ClearBlobFiles();

            Dictionary<string, (byte[] Content, byte[] Hash)> customBlobs = new();
            string[] blobNames = new[]
            {
                "a/b/c/d/e",
                "a/b/d/e",
                "a/c",
                "b/c/d",
                "d/e/f",
                "a/b/c/e/f",
            };

            for (int i = 0; i < blobNames.Length; i++)
            {
                int contentLength = RandomNumberGenerator.GetInt32(100, 400);
                byte[] content = RandomNumberGenerator.GetBytes(contentLength);
                byte[] hash = content.ToMD5();

                customBlobs[blobNames[i]] = (content, hash);
                await blobClient.UploadBlobAsync(blobNames[i], BinaryData.FromBytes(content));
            }

            AzureBlobProvider blobProvider = Create(false, new[] { "/a/b/d/", "/a/b/c/d/", "/b/c/" });
            Assert.IsInstanceOfType(blobProvider.GetFileInfo("a/b/c/d/f"), typeof(NotFoundBlobInfo));
            Assert.IsInstanceOfType(blobProvider.GetFileInfo("/a/b/c/e/f"), typeof(NotFoundBlobInfo));
            Assert.IsInstanceOfType(blobProvider.GetFileInfo("a/b/d/e"), typeof(AzureBlobInfo));
            Assert.IsInstanceOfType(blobProvider.GetFileInfo("/a/b/c/d/e"), typeof(AzureBlobInfo));
            Assert.ThrowsException<ArgumentException>(() => blobProvider.GetFileInfo("/a/b/c/d/../e/f"), "Path cannot include '/../'.");

            AssertDirectoryContents(blobProvider.GetDirectoryContents("/"), new[] { "a/", "b/" });
            AssertDirectoryContents(blobProvider.GetDirectoryContents("b/"), new[] { "b/c/" });
            var dirs = AssertDirectoryContents(blobProvider.GetDirectoryContents("/a/b/"), new[] { "a/b/c/", "a/b/d/" });
            AssertDirectoryContents((IDirectoryContents)dirs[1], new[] { "a/b/d/e" });
            var dir2 = AssertDirectoryContents((IDirectoryContents)dirs[0], new[] { "a/b/c/d/" });
            AssertDirectoryContents((IDirectoryContents)dir2[0], new[] { "a/b/c/d/e" });
        }

        private async Task ClearBlobFiles()
        {
            List<string> blobNames = new();
            await foreach (BlobItem blob in blobClient.GetBlobsAsync())
            {
                blobNames.Add(blob.Name);
            }

            foreach (string blobName in blobNames)
            {
                await blobClient.DeleteBlobAsync(blobName);
            }
        }

        private List<IFileInfo> AssertDirectoryContents(
            IDirectoryContents directory,
            string[] fileNames,
            Action<IFileInfo, string> fileAction = null,
            Action<IFileInfo, string> directoryAction = null)
        {
            List<IFileInfo> files = directory.OrderBy(f => f.Name).ToList();
            Assert.AreEqual(fileNames.Length, files.Count);
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (fileNames[i].EndsWith('/'))
                {
                    Assert.IsTrue(files[i].IsDirectory);
                    Assert.IsInstanceOfType(files[i], typeof(IDirectoryContents));
                    Assert.AreEqual(blobClient.Uri + "/" + fileNames[i], files[i].ToString());
                    directoryAction?.Invoke(files[i], fileNames[i]);
                }
                else
                {
                    Assert.IsFalse(files[i].IsDirectory);
                    Assert.AreEqual(Path.GetFileName(fileNames[i]), files[i].Name);
                    Assert.IsInstanceOfType(files[i], typeof(IBlobInfo));
                    fileAction?.Invoke(files[i], fileNames[i]);
                }
            }

            return files;
        }
    }
}
