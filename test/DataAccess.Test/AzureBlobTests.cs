using Azure.Storage.Blobs;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class AzureBlobTests
    {
        private BlobContainerClient blobClient;
        private PhysicalMutableFileProvider physicalFileProvider;
        private AzureBlobFileProvider fileProvider;

        [TestInitialize]
        public async Task TestInitialize()
        {
            blobClient = new BlobContainerClient("UseDevelopmentStorage=true", "localtest");
            await TestCleanup();

            await blobClient.CreateAsync();
            Directory.CreateDirectory("./BlobContainerTestRoot");
            physicalFileProvider = new PhysicalMutableFileProvider(Path.GetFullPath("./BlobContainerTestRoot"));
            fileProvider = new AzureBlobFileProvider(blobClient, physicalFileProvider, default); // AccessTier.Hot);
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
        public async Task UploadAsync()
        {
            IFileInfo file = await fileProvider.WriteStringAsync("/hello.txt", "hello, world!");

            Assert.AreEqual("hello.txt", file.Name);
            Assert.AreEqual(13, file.Length);

            File.Delete(file.PhysicalPath);

            IFileInfo file1 = await fileProvider.GetFileInfoAsync("/hello.txt");

            Assert.AreEqual("hello.txt", file1.Name);
            Assert.AreEqual(13, file1.Length);
        }
    }
}
