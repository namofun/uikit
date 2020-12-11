using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class RoleDefinitionTests
    {
        [DataTestMethod]
        [DataRow("9805fda4-f300-ff78-a6d8-faf0d8e418fd", 1, "Administrative User", "Administrator", "admin")]
        [DataRow("f1722fb1-cd10-256b-48bb-71afd116ae66", 2, "Blocked User", "Blocked", "blocked")]
        [DataRow("da469e40-6c61-872e-63e3-d17d317e4700", 666, "Forecast User", "Forecast", "forecast")]
        public void EnsureConcurrencyStamp(string concurrencyStamp, int id, string description, string name, string shortName)
        {
            var attr = new RoleDefinitionAttribute(id, name, shortName, description);
            var genVal = attr.GenerateDefaultConcurrencyStamp();
            Assert.AreEqual(concurrencyStamp, genVal);
        }
    }
}
