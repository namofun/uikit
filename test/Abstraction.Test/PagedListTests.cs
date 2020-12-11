using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class PagedListTests
    {
        [DataTestMethod]
        [DataRow(1000, 300, 4)]
        [DataRow(1000, 1, 1000)]
        [DataRow(10, 100, 1)]
        [DataRow(100, 100, 1)]
        [DataRow(101, 100, 2)]
        [DataRow(200, 100, 2)]
        public void EnsureCalculate(int totalItems, int perPage, int totalPages)
        {
            var pagedList = new PagedViewList<int>(Array.Empty<int>(), 1, totalItems, perPage);
            Assert.AreEqual(totalPages, pagedList.TotalPage);

            var newList = pagedList.As(i => 1L * i * i);
            Assert.AreEqual(totalPages, newList.TotalPage);
        }
    }
}
