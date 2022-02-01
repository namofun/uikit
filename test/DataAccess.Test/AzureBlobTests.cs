﻿using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class AzureBlobTests
    {
        private BlobContainerClient blobClient;

        private AzureBlobProvider Create(bool allowAutoCache = false)
        {
            return new(
                blobClient,
                Path.GetFullPath("./BlobContainerTestRoot"),
                default,
                allowAutoCache);
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
        }
    }
}
