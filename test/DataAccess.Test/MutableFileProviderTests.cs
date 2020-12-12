using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class MutableFileProviderTests
    {
        public string DirectoryPath { get; set; }

        public PhysicalMutableFileProvider PhysicalMutableFileProvider { get; set; }

        public IMutableFileProvider MutableFileProvider => PhysicalMutableFileProvider;

        [TestInitialize]
        public void InitializeDirectory()
        {
            do DirectoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            while (Directory.Exists(DirectoryPath));

            Directory.CreateDirectory(DirectoryPath);
            PhysicalMutableFileProvider = new PhysicalMutableFileProvider(DirectoryPath);
        }

        private byte[] GenerateByteArray(int length)
        {
            var bytes = new byte[length];
            var rng = new Random();
            rng.NextBytes(bytes);
            return bytes;
        }

        private string GenerateString(int length)
        {
            var bytes = new char[length];
            var rng = new Random();
            var src = "1234567890!@#$%^&*()QWERTYUIOPqwertyuiopASDFGHJKLasdfghjklZXCVBNMzxcvbnm-=_+[]\\{}|;':\",./<>?";
            for (int i = 0; i < length; i++)
                bytes[i] = src[rng.Next(src.Length)];
            return new string(bytes);
        }

        [TestMethod]
        public async Task WriteString_BIGsmall()
        {
            string subpath;
            do subpath = Guid.NewGuid().ToString();
            while (File.Exists(Path.Combine(DirectoryPath, subpath)));

            var bigBlob = GenerateString(1000);
            await MutableFileProvider.WriteStringAsync(subpath, bigBlob);
            var read = await File.ReadAllBytesAsync(Path.Combine(DirectoryPath, subpath));
            Assert.IsTrue(Encoding.UTF8.GetBytes(bigBlob).SequenceEqual(read));

            var smallBlob = GenerateString(10);
            await MutableFileProvider.WriteStringAsync(subpath, smallBlob);
            read = await File.ReadAllBytesAsync(Path.Combine(DirectoryPath, subpath));
            Assert.IsTrue(Encoding.UTF8.GetBytes(smallBlob).SequenceEqual(read));
        }

        [TestMethod]
        public async Task WriteBinary_BIGsmall()
        {
            string subpath;
            do subpath = Guid.NewGuid().ToString();
            while (File.Exists(Path.Combine(DirectoryPath, subpath)));

            var bigBlob = GenerateByteArray(1000);
            await MutableFileProvider.WriteBinaryAsync(subpath, bigBlob);
            var read = await File.ReadAllBytesAsync(Path.Combine(DirectoryPath, subpath));
            Assert.IsTrue(bigBlob.SequenceEqual(read));

            var smallBlob = GenerateByteArray(10);
            await MutableFileProvider.WriteBinaryAsync(subpath, smallBlob);
            read = await File.ReadAllBytesAsync(Path.Combine(DirectoryPath, subpath));
            Assert.IsTrue(smallBlob.SequenceEqual(read));
        }

        [TestMethod]
        public async Task WriteStream_BIGsmall()
        {
            string subpath;
            do subpath = Guid.NewGuid().ToString();
            while (File.Exists(Path.Combine(DirectoryPath, subpath)));

            var bigBlob = GenerateByteArray(1000);
            var stream = new MemoryStream(bigBlob, false);
            await MutableFileProvider.WriteStreamAsync(subpath, stream);
            await stream.DisposeAsync();
            var read = await File.ReadAllBytesAsync(Path.Combine(DirectoryPath, subpath));
            Assert.IsTrue(bigBlob.SequenceEqual(read));

            var smallBlob = GenerateByteArray(10);
            stream = new MemoryStream(smallBlob, false);
            await MutableFileProvider.WriteStreamAsync(subpath, stream);
            await stream.DisposeAsync();
            read = await File.ReadAllBytesAsync(Path.Combine(DirectoryPath, subpath));
            Assert.IsTrue(smallBlob.SequenceEqual(read));
        }

        [TestMethod]
        public async Task RemoveFile()
        {
            string subpath;
            do subpath = Guid.NewGuid().ToString();
            while (File.Exists(Path.Combine(DirectoryPath, subpath)));

            Assert.IsFalse(await MutableFileProvider.RemoveFileAsync(subpath));
            await File.WriteAllBytesAsync(Path.Combine(DirectoryPath, subpath), Array.Empty<byte>());
            Assert.IsTrue(await MutableFileProvider.RemoveFileAsync(subpath));
            Assert.IsFalse(await MutableFileProvider.RemoveFileAsync(subpath));
        }

        [TestCleanup]
        public void DisposeDirectory()
        {
            Directory.Delete(DirectoryPath, true);
            PhysicalMutableFileProvider.Dispose();
            PhysicalMutableFileProvider = null;
            DirectoryPath = null;
        }
    }
}
