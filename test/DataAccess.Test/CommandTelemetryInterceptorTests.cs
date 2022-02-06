using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SatelliteSite.Tests
{
    [TestClass]
    public class CommandTelemetryInterceptorTests
    {
        private SqliteConnection connection;

        [TestInitialize]
        public void TestInitialize()
        {
            connection = new("Filename=:memory:");
            connection.Open();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            connection.Dispose();
            connection = null;
        }

        private class Context : DbContext
        {
            public Context(DbContextOptions options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder model)
            {
                model.Entity<Configuration>(entity =>
                {
                    entity.HasKey(e => e.Name);
                });
            }
        }

        private class SqlClientDiagnosticObserver : IObserver<KeyValuePair<string, object>>
        {
            private readonly List<KeyValuePair<string, object>> _results;

            public SqlClientDiagnosticObserver(List<KeyValuePair<string, object>> results)
            {
                _results = results;
            }

            public void OnCompleted()
            {
                lock (_results) _results.Add(new("OnCompleted", null));
            }

            public void OnError(Exception error)
            {
                lock (_results) _results.Add(new("OnError", error));
            }

            public void OnNext(KeyValuePair<string, object> value)
            {
                lock (_results) _results.Add(value);
            }
        }

        private class DiagnosticListenerObserver : IObserver<DiagnosticListener>, IDisposable
        {
            private readonly List<KeyValuePair<string, object>> _results;
            private readonly List<IDisposable> _disposables = new();

            public DiagnosticListenerObserver(List<KeyValuePair<string, object>> values)
            {
                _results = values;
            }

            public void Dispose()
            {
                _disposables.ForEach(e => e.Dispose());
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(DiagnosticListener value)
            {
                if (value.Name == "SqlClientDiagnosticListener")
                {
                    _disposables.Add(value.Subscribe(new SqlClientDiagnosticObserver(_results)));
                }
            }
        }

        const string SqlBeforeExecuteCommand = "System.Data.SqlClient.WriteCommandBefore";
        const string SqlAfterExecuteCommand = "System.Data.SqlClient.WriteCommandAfter";
        const string SqlErrorExecuteCommand = "System.Data.SqlClient.WriteCommandError";

        const string SqlBeforeOpenConnection = "System.Data.SqlClient.WriteConnectionOpenBefore";
        const string SqlAfterOpenConnection = "System.Data.SqlClient.WriteConnectionOpenAfter";
        const string SqlErrorOpenConnection = "System.Data.SqlClient.WriteConnectionOpenError";

        const string SqlBeforeCloseConnection = "System.Data.SqlClient.WriteConnectionCloseBefore";
        const string SqlAfterCloseConnection = "System.Data.SqlClient.WriteConnectionCloseAfter";
        const string SqlErrorCloseConnection = "System.Data.SqlClient.WriteConnectionCloseError";

        const string SqlBeforeCommitTransaction = "System.Data.SqlClient.WriteTransactionCommitBefore";
        const string SqlAfterCommitTransaction = "System.Data.SqlClient.WriteTransactionCommitAfter";
        const string SqlErrorCommitTransaction = "System.Data.SqlClient.WriteTransactionCommitError";

        const string SqlBeforeRollbackTransaction = "System.Data.SqlClient.WriteTransactionRollbackBefore";
        const string SqlAfterRollbackTransaction = "System.Data.SqlClient.WriteTransactionRollbackAfter";
        const string SqlErrorRollbackTransaction = "System.Data.SqlClient.WriteTransactionRollbackError";

        [TestMethod]
        public async Task ShouldReceive_ExecuteCommand()
        {
            List<KeyValuePair<string, object>> observed = await TestBootstrapper(async ctx =>
            {
                await ctx.Set<Configuration>().ToListAsync();
                try
                {
                    await ctx.Database.ExecuteSqlRawAsync("HELLO");
                }
                catch
                {
                }
            });

            Assert.AreEqual(4, observed.Count);
            Assert.AreEqual(SqlBeforeExecuteCommand, observed[0].Key);
            Assert.AreEqual(SqlAfterExecuteCommand, observed[1].Key);
            Assert.AreEqual(SqlBeforeExecuteCommand, observed[2].Key);
            Assert.AreEqual(SqlErrorExecuteCommand, observed[3].Key);
            Assert.AreEqual(GetOperationId(observed[0].Value), GetOperationId(observed[1].Value));
            Assert.AreEqual(GetOperationId(observed[2].Value), GetOperationId(observed[3].Value));
        }

        [TestMethod]
        public async Task DbTransactionInterceptor()
        {
            List<KeyValuePair<string, object>> observed = await TestBootstrapper(async ctx =>
            {
                IDbTransactionInterceptor interceptor = (IDbTransactionInterceptor)DiagnosticDbInterceptor.Instance;
                DateTimeOffset time = DateTimeOffset.UtcNow;
                TimeSpan span = TimeSpan.FromSeconds(1);
                Guid conId = Guid.NewGuid();

                FakeTransaction t = new(connection, IsolationLevel.ReadCommitted);
                await interceptor.TransactionCommittingAsync(t, new(null, null, t, ctx, t.Id, conId, true, time), default);
                await interceptor.TransactionCommittedAsync(t, new(null, null, t, ctx, t.Id, conId, true, time, span));

                t = new(connection, IsolationLevel.ReadCommitted);
                await interceptor.TransactionCommittingAsync(t, new(null, null, t, ctx, t.Id, conId, true, time), default);
                await interceptor.TransactionFailedAsync(t, new(null, null, t, ctx, t.Id, conId, true, "Commit", new Exception(), time, span));

                t = new(connection, IsolationLevel.ReadCommitted);
                await interceptor.TransactionRollingBackAsync(t, new(null, null, t, ctx, t.Id, conId, true, time), default);
                await interceptor.TransactionRolledBackAsync(t, new(null, null, t, ctx, t.Id, conId, true, time, span));

                t = new(connection, IsolationLevel.ReadCommitted);
                await interceptor.TransactionRollingBackAsync(t, new(null, null, t, ctx, t.Id, conId, true, time), default);
                await interceptor.TransactionFailedAsync(t, new(null, null, t, ctx, t.Id, conId, true, "Rollback", new Exception(), time, span));

                // no-op
                await interceptor.ReleasedSavepointAsync(null, null);
                await interceptor.ReleasingSavepointAsync(null, null, default);
                await interceptor.RolledBackToSavepointAsync(null, null);
                await interceptor.RollingBackToSavepointAsync(null, null, default);
                await interceptor.TransactionUsedAsync(null, null, null);
                await interceptor.TransactionStartingAsync(null, null, default);
                await interceptor.TransactionStartedAsync(null, null, null);
            });

            Assert.AreEqual(8, observed.Count);
            Assert.AreEqual(SqlBeforeCommitTransaction, observed[0].Key);
            Assert.AreEqual(SqlAfterCommitTransaction, observed[1].Key);
            Assert.AreEqual(SqlBeforeCommitTransaction, observed[2].Key);
            Assert.AreEqual(SqlErrorCommitTransaction, observed[3].Key);
            Assert.AreEqual(SqlBeforeRollbackTransaction, observed[4].Key);
            Assert.AreEqual(SqlAfterRollbackTransaction, observed[5].Key);
            Assert.AreEqual(SqlBeforeRollbackTransaction, observed[6].Key);
            Assert.AreEqual(SqlErrorRollbackTransaction, observed[7].Key);
            Assert.AreEqual(GetOperationId(observed[0].Value), GetOperationId(observed[1].Value));
            Assert.AreEqual(GetOperationId(observed[2].Value), GetOperationId(observed[3].Value));
            Assert.AreEqual(GetOperationId(observed[4].Value), GetOperationId(observed[5].Value));
            Assert.AreEqual(GetOperationId(observed[6].Value), GetOperationId(observed[7].Value));
        }

        [TestMethod]
        public async Task DbConnectionInterceptor()
        {
            List<KeyValuePair<string, object>> observed = await TestBootstrapper(async ctx =>
            {
                IDbConnectionInterceptor interceptor = (IDbConnectionInterceptor)DiagnosticDbInterceptor.Instance;
                DateTimeOffset time = DateTimeOffset.UtcNow;
                TimeSpan span = TimeSpan.FromSeconds(1);

                FakeConnection c = new();
                await interceptor.ConnectionOpeningAsync(c, new(null, null, c, ctx, c.Id, true, time), default);
                await interceptor.ConnectionOpenedAsync(c, new(null, null, c, ctx, c.Id, true, time, span));

                c = new();
                await interceptor.ConnectionOpeningAsync(c, new(null, null, c, ctx, c.Id, true, time), default);
                await interceptor.ConnectionFailedAsync(c, new(null, null, c, ctx, c.Id, new Exception(), true, time, span));

                c = new() { State2 = ConnectionState.Open };
                await interceptor.ConnectionClosingAsync(c, new(null, null, c, ctx, c.Id, true, time), default);
                await interceptor.ConnectionClosedAsync(c, new(null, null, c, ctx, c.Id, true, time, span));

                c = new() { State2 = ConnectionState.Open };
                await interceptor.ConnectionClosingAsync(c, new(null, null, c, ctx, c.Id, true, time), default);
                await interceptor.ConnectionFailedAsync(c, new(null, null, c, ctx, c.Id, new Exception(), true, time, span));
            });

            Assert.AreEqual(8, observed.Count);
            Assert.AreEqual(SqlBeforeOpenConnection, observed[0].Key);
            Assert.AreEqual(SqlAfterOpenConnection, observed[1].Key);
            Assert.AreEqual(SqlBeforeOpenConnection, observed[2].Key);
            Assert.AreEqual(SqlErrorOpenConnection, observed[3].Key);
            Assert.AreEqual(SqlBeforeCloseConnection, observed[4].Key);
            Assert.AreEqual(SqlAfterCloseConnection, observed[5].Key);
            Assert.AreEqual(SqlBeforeCloseConnection, observed[6].Key);
            Assert.AreEqual(SqlErrorCloseConnection, observed[7].Key);
            Assert.AreEqual(GetOperationId(observed[0].Value), GetOperationId(observed[1].Value));
            Assert.AreEqual(GetOperationId(observed[2].Value), GetOperationId(observed[3].Value));
            Assert.AreEqual(GetOperationId(observed[4].Value), GetOperationId(observed[5].Value));
            Assert.AreEqual(GetOperationId(observed[6].Value), GetOperationId(observed[7].Value));
        }

        [TestMethod]
        public async Task DbCommandInterceptor()
        {
            List<KeyValuePair<string, object>> observed = await TestBootstrapper(async ctx =>
            {
                IDbCommandInterceptor interceptor = (IDbCommandInterceptor)DiagnosticDbInterceptor.Instance;
                DateTimeOffset time = DateTimeOffset.UtcNow;
                TimeSpan span = TimeSpan.FromSeconds(1);
                Guid conId = Guid.NewGuid();

                interceptor.CommandCreated(null, null);
                interceptor.CommandCreating(null, default);
                interceptor.DataReaderDisposing(null, null, default);

                FakeCommand c = new();
                await interceptor.ScalarExecutingAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteScalar, c.Id, conId, true, false, time, CommandSource.FromSqlQuery), default);
                await interceptor.ScalarExecutedAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteScalar, c.Id, conId, null, true, false, time, span, CommandSource.FromSqlQuery), null);

                c = new();
                await interceptor.ScalarExecutingAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteScalar, c.Id, conId, true, false, time, CommandSource.FromSqlQuery), default);
                await interceptor.CommandFailedAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteScalar, c.Id, conId, new Exception(), true, false, time, span, CommandSource.FromSqlQuery));

                c = new();
                await interceptor.NonQueryExecutingAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteNonQuery, c.Id, conId, true, false, time, CommandSource.ExecuteSqlRaw), default);
                await interceptor.NonQueryExecutedAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteNonQuery, c.Id, conId, null, true, false, time, span, CommandSource.ExecuteSqlRaw), 0);

                c = new();
                await interceptor.NonQueryExecutingAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteScalar, c.Id, conId, true, false, time, CommandSource.ExecuteSqlRaw), default);
                await interceptor.CommandFailedAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteScalar, c.Id, conId, new Exception(), true, false, time, span, CommandSource.ExecuteSqlRaw));

                c = new();
                await interceptor.ReaderExecutingAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteReader, c.Id, conId, true, false, time, CommandSource.LinqQuery), default);
                await interceptor.ReaderExecutedAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteReader, c.Id, conId, null, true, false, time, span, CommandSource.LinqQuery), null);

                c = new();
                await interceptor.ReaderExecutingAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteReader, c.Id, conId, true, false, time, CommandSource.LinqQuery), default);
                await interceptor.CommandFailedAsync(c, new(null, null, connection, c, ctx, DbCommandMethod.ExecuteReader, c.Id, conId, new Exception(), true, false, time, span, CommandSource.LinqQuery));
            });

            Assert.AreEqual(12, observed.Count);
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(SqlBeforeExecuteCommand, observed[i * 2].Key);
                Assert.AreEqual(i % 2 == 0 ? SqlAfterExecuteCommand : SqlErrorExecuteCommand, observed[i * 2 + 1].Key);
                Assert.AreEqual(GetOperationId(observed[i * 2].Value), GetOperationId(observed[i * 2 + 1].Value));
            }
        }

        private async Task<List<KeyValuePair<string, object>>> TestBootstrapper(Func<Context, Task> worker)
        {
            List<KeyValuePair<string, object>> observed = new();
            using var subscription = DiagnosticListener.AllListeners.Subscribe(new DiagnosticListenerObserver(observed));

            using var host = new SimpleHostBuilder()
                .AddDatabase<Context>(b => b.UseSqlite(connection, b => b.UseBulk()))
                .Build()
                .EnsureCreated<Context>();

            using var scope = host.Services.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<Context>();

            observed.Clear();

            await worker(context);

            return observed;
        }

        private static string GetOperationId(object @event)
        {
            return @event.GetType().GetProperty("OperationId").GetValue(@event).ToString();
        }

        private class FakeTransaction : DbTransaction
        {
            public override IsolationLevel IsolationLevel { get; }
            protected override DbConnection DbConnection { get; }
            public Guid Id { get; }
            public override void Commit() { }
            public override void Rollback() { }

            public FakeTransaction(DbConnection connection, IsolationLevel isolationLevel)
            {
                DbConnection = connection;
                IsolationLevel = isolationLevel;
                Id = Guid.NewGuid();
            }
        }

        private class FakeConnection : DbConnection
        {
            private ConnectionState state;

            public Guid Id { get; } = Guid.NewGuid();
            public override string ConnectionString { get; set; } = string.Empty;
            public override string Database => string.Empty;
            public override string DataSource => string.Empty;
            public override string ServerVersion => string.Empty;
            public override ConnectionState State => state;
            public ConnectionState State2 { get => state; set => state = value; }
            public override void ChangeDatabase(string databaseName) { }
            public override void Close() { }
            public override void Open() { }
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => null;
            protected override DbCommand CreateDbCommand() => null;
        }

        private class FakeCommand : DbCommand
        {
            public Guid Id { get; } = Guid.NewGuid();
            public override string CommandText { get; set; } = "SELECT 1";
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override bool DesignTimeVisible { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbParameterCollection DbParameterCollection { get; }
            protected override DbTransaction DbTransaction { get; set; }
            public override void Cancel() { }
            public override int ExecuteNonQuery() => 0;
            public override object ExecuteScalar() => null;
            public override void Prepare() { }
            protected override DbParameter CreateDbParameter() => null;
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => null;
        }
    }
}
