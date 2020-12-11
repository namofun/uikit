using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class ConfigurationItemTests
    {
        [DataTestMethod]
        [DataRow(typeof(int), 1)]
        [DataRow(typeof(int), 10000)]
        //[DataRow(typeof(int?), null)]
        [DataRow(typeof(string), "SOME STRING")]
        [DataRow(typeof(string), null)]
        [DataRow(typeof(DateTimeOffset?), null)]
        [DataRow(typeof(bool), true)]
        [DataRow(typeof(bool), false)]
        //[DataRow(typeof(bool?), null)]
        public void ShouldSuccess(Type type, object value)
        {
            var expectedConverted = "null";
            if (value != null) expectedConverted = value.ToJson();

            var e = new ConfigurationItemAttribute(
                0, string.Empty, string.Empty,
                type, value, string.Empty)
                .ToEntity();

            Assert.AreEqual(expectedConverted, e.Value);
        }

        [DataTestMethod]
        [DataRow(typeof(int), null)]
        [DataRow(typeof(int), "HELLO")]
        [DataRow(typeof(bool), null)]
        [DataRow(typeof(string), 66666)]
        [DataRow(typeof(DateTimeOffset), 5487)]
        [DataRow(typeof(DateTimeOffset), "OTHER STRING")]
        public void ShouldFail(Type type, object value)
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                new ConfigurationItemAttribute(
                    0, string.Empty, string.Empty,
                    type, value, string.Empty)
                    .ToEntity();
            });
        }
    }
}
