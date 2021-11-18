using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class GitVersionDiscoveryTests
    {
        [TestMethod]
        public void NotUnknownBranch()
        {
            Assert.AreNotEqual(
                "unknown",
                typeof(GitVersionAttribute).Assembly
                    .GetCustomAttribute<GitVersionAttribute>()?
                    .Branch ?? "unknown");
        }
    }
}
