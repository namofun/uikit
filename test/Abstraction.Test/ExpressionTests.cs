using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class ExpressionTests
    {
        [TestMethod]
        public void StaticMembersNoThrow()
        {
            Expression<Func<TimeSpan, DateTimeOffset>> f = s => DateTimeOffset.Now + s;
            f.Body.ReplaceWith(f.Parameters[0], Expression.Constant(TimeSpan.Zero, typeof(TimeSpan)));
        }
    }
}
