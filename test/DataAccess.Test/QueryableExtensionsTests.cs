using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class QueryableExtensionsTests
    {
        private Context context;

        [TestInitialize]
        public void TestInitialize()
        {
            context = new Context(
                new DbContextOptionsBuilder<Context>()
                    .UseInMemoryDatabase("Test", b => b.UseBulk())
                    .Options);

            context.Database.EnsureCreated();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            context.Dispose();
            context = null;
        }

        private class Context : DbContext
        {
            public Context(DbContextOptions options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder model)
            {
                model.Entity<Entity>(entity =>
                {
                    entity.HasKey(e => e.Id);

                    entity.HasMany(e => e.SubEntities)
                        .WithOne()
                        .OnDelete(DeleteBehavior.Cascade);
                });
            }
        }

        private class SubEntity
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Entity
        {
            public int Id { get; set; }
            public ICollection<SubEntity> SubEntities { get; set; }
        }

        [TestMethod]
        public void QueryableIfs()
        {
            var root = context.Set<Entity>().AsQueryable();
            Assert.AreNotEqual(ExpressionType.Call, root.Expression.NodeType);

            var testQueryable = root.WhereIf(true, c => true);
            Assert.AreEqual(ExpressionType.Call, testQueryable.Expression.NodeType);
            Assert.AreEqual("Where", ((MethodCallExpression)testQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)testQueryable.Expression).Arguments[0]);

            testQueryable = root.WhereIf(false, c => true);
            Assert.AreSame(root.Expression, testQueryable.Expression);

            testQueryable = root.SkipIf(10);
            Assert.AreEqual(ExpressionType.Call, testQueryable.Expression.NodeType);
            Assert.AreEqual("Skip", ((MethodCallExpression)testQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)testQueryable.Expression).Arguments[0]);

            testQueryable = root.SkipIf(null);
            Assert.AreSame(root.Expression, testQueryable.Expression);

            testQueryable = root.TakeIf(10);
            Assert.AreEqual(ExpressionType.Call, testQueryable.Expression.NodeType);
            Assert.AreEqual("Take", ((MethodCallExpression)testQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)testQueryable.Expression).Arguments[0]);

            Assert.ThrowsException<InvalidOperationException>(() => root.SkipIf(-1));
            Assert.ThrowsException<InvalidOperationException>(() => root.TakeIf(-1));

            testQueryable = root.TakeIf(null);
            Assert.AreSame(root.Expression, testQueryable.Expression);

            testQueryable = root.IncludeIf(true, e => e.SubEntities);
            Assert.AreEqual(ExpressionType.Call, testQueryable.Expression.NodeType);
            Assert.AreEqual("Include", ((MethodCallExpression)testQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)testQueryable.Expression).Arguments[0]);

            testQueryable = root.IncludeIf(false, e => e.SubEntities);
            Assert.AreSame(root.Expression, testQueryable.Expression);

            testQueryable = root.SelectIf(true, e => new() { Id = e.Id });
            Assert.AreEqual(ExpressionType.Call, testQueryable.Expression.NodeType);
            Assert.AreEqual("Select", ((MethodCallExpression)testQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)testQueryable.Expression).Arguments[0]);

            testQueryable = root.SelectIf(false, e => new() { Id = e.Id });
            Assert.AreSame(root.Expression, testQueryable.Expression);

            var orderedQueryable = root.OrderByBoolean(e => e.Id, ascending: true);
            Assert.AreEqual(ExpressionType.Call, orderedQueryable.Expression.NodeType);
            Assert.AreEqual("OrderBy", ((MethodCallExpression)orderedQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)orderedQueryable.Expression).Arguments[0]);

            orderedQueryable = root.OrderByBoolean(e => e.Id, ascending: false);
            Assert.AreEqual(ExpressionType.Call, orderedQueryable.Expression.NodeType);
            Assert.AreEqual("OrderByDescending", ((MethodCallExpression)orderedQueryable.Expression).Method.Name);
            Assert.AreSame(root.Expression, ((MethodCallExpression)orderedQueryable.Expression).Arguments[0]);

            var thenByRoot = root.OrderBy(e => e.Id);
            orderedQueryable = thenByRoot.ThenByBoolean(e => e.Id, ascending: true);
            Assert.AreEqual(ExpressionType.Call, orderedQueryable.Expression.NodeType);
            Assert.AreEqual("ThenBy", ((MethodCallExpression)orderedQueryable.Expression).Method.Name);
            Assert.AreSame(thenByRoot.Expression, ((MethodCallExpression)orderedQueryable.Expression).Arguments[0]);

            orderedQueryable = thenByRoot.ThenByBoolean(e => e.Id, ascending: false);
            Assert.AreEqual(ExpressionType.Call, orderedQueryable.Expression.NodeType);
            Assert.AreEqual("ThenByDescending", ((MethodCallExpression)orderedQueryable.Expression).Method.Name);
            Assert.AreSame(thenByRoot.Expression, ((MethodCallExpression)orderedQueryable.Expression).Arguments[0]);
        }

        [TestMethod]
        public async Task QueryableToThingsAsync()
        {
            context.Set<Entity>().RemoveRange(await context.Set<Entity>().ToListAsync());
            await context.SaveChangesAsync();

            context.Set<Entity>().AddRange(Enumerable.Range(1, 13).Select(e => new Entity() { Id = e }));
            await context.SaveChangesAsync();

            var a = await context.Set<Entity>().Select(e => e.Id % 8).ToHashSetAsync();
            Assert.IsTrue(a.SetEquals(Enumerable.Range(0, 8)));

            var b = await context.Set<Entity>().OrderBy(e => e.Id).ToPagedListAsync(1, 5);
            Assert.AreEqual(5, b.Count);
            Assert.AreEqual(3, b.TotalPage);
            Assert.AreEqual(13, b.TotalCount);
            Assert.IsTrue(b.Select(e => e.Id).SequenceEqual(Enumerable.Range(1, 5)));

            b = await context.Set<Entity>().OrderBy(e => e.Id).ToPagedListAsync(3, 5);
            Assert.AreEqual(3, b.Count);
            Assert.IsTrue(b.Select(e => e.Id).SequenceEqual(Enumerable.Range(11, 3)));

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => (null as IQueryable<Entity>).ToPagedListAsync(1, 1));
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => context.Set<Entity>().ToPagedListAsync(-1, 1));
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() => context.Set<Entity>().ToPagedListAsync(1, -1));

            var c = await context.Set<Entity>().ToLookupAsync(a => a.Id % 3, a => a);
            Assert.AreEqual(3, c.Count);
            Assert.AreEqual(5, c[1].Count());
            Assert.AreEqual(4, c[2].Count());
            Assert.AreEqual(4, c[0].Count());
            Assert.AreEqual(0, c[3].Count());

            Assert.IsTrue(Enumerable.Range(11, 3).SequenceEqual(
                await context.Set<Entity>().OrderBy(e => e.Id).ToPagedListAsync(3, 5).TransformAfterAcquire(e => e.Id)));

            Assert.IsTrue(Enumerable.Range(1, 13).SequenceEqual(
                await context.Set<Entity>().OrderBy(e => e.Id).ToListAsync().TransformAfterAcquire(e => e.Id)));
        }

        [TestMethod]
        public void LeftJoin()
        {
            var root = context.Set<Entity>().AsQueryable();
            Assert.AreNotEqual(ExpressionType.Call, root.Expression.NodeType);

            var q = root.LeftJoin(root, e => e.Id % 3, e => e.Id, (l, r) => new { l, r });
            Assert.AreEqual(ExpressionType.Call, q.Expression.NodeType);
            Assert.AreEqual("SelectMany", ((MethodCallExpression)q.Expression).Method.Name);

            var argExp = (MethodCallExpression)q.Expression;
            Assert.AreEqual(ExpressionType.Call, argExp.Arguments[0].NodeType);
            Assert.AreEqual("GroupJoin", ((MethodCallExpression)argExp.Arguments[0]).Method.Name);

            Assert.AreEqual(ExpressionType.Quote, argExp.Arguments[1].NodeType);
            Assert.AreEqual(ExpressionType.Lambda, ((UnaryExpression)argExp.Arguments[1]).Operand.NodeType);
            Assert.AreEqual(ExpressionType.Call, ((LambdaExpression)((UnaryExpression)argExp.Arguments[1]).Operand).Body.NodeType);
            Assert.AreEqual("DefaultIfEmpty", ((MethodCallExpression)((LambdaExpression)((UnaryExpression)argExp.Arguments[1]).Operand).Body).Method.Name);
        }
    }
}
