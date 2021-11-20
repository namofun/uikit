namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    [SuppressMessage("Style", "IDE0037:UseInferredName", Justification = "Code Style")]
    public class DiagnosticDbInterceptor : IDbCommandInterceptor, IDbConnectionInterceptor, IDbTransactionInterceptor
    {
        public static IInterceptor Instance { get; } = new DiagnosticDbInterceptor();

        #region DiagnosticListener

        private const string DiagnosticListenerName = "SqlClientDiagnosticListener";

        private const string SqlClientPrefix = "System.Data.SqlClient.";

        private const string SqlBeforeExecuteCommand = SqlClientPrefix + nameof(WriteCommandBefore);
        private const string SqlAfterExecuteCommand = SqlClientPrefix + nameof(WriteCommandAfter);
        private const string SqlErrorExecuteCommand = SqlClientPrefix + nameof(WriteCommandError);

        private const string SqlBeforeOpenConnection = SqlClientPrefix + nameof(WriteConnectionOpenBefore);
        private const string SqlAfterOpenConnection = SqlClientPrefix + nameof(WriteConnectionOpenAfter);
        private const string SqlErrorOpenConnection = SqlClientPrefix + nameof(WriteConnectionOpenError);

        private const string SqlBeforeCloseConnection = SqlClientPrefix + nameof(WriteConnectionCloseBefore);
        private const string SqlAfterCloseConnection = SqlClientPrefix + nameof(WriteConnectionCloseAfter);
        private const string SqlErrorCloseConnection = SqlClientPrefix + nameof(WriteConnectionCloseError);

        private const string SqlBeforeCommitTransaction = SqlClientPrefix + nameof(WriteTransactionCommitBefore);
        private const string SqlAfterCommitTransaction = SqlClientPrefix + nameof(WriteTransactionCommitAfter);
        private const string SqlErrorCommitTransaction = SqlClientPrefix + nameof(WriteTransactionCommitError);

        private const string SqlBeforeRollbackTransaction = SqlClientPrefix + nameof(WriteTransactionRollbackBefore);
        private const string SqlAfterRollbackTransaction = SqlClientPrefix + nameof(WriteTransactionRollbackAfter);
        private const string SqlErrorRollbackTransaction = SqlClientPrefix + nameof(WriteTransactionRollbackError);

        private static readonly DiagnosticListener _diagnosticListener = new DiagnosticListener(DiagnosticListenerName);

        private static Guid CreateComposedGuid(Guid guid, byte offset = 1)
        {
            var arr = guid.ToByteArray();
            arr[0] += offset;
            return new Guid(arr);
        }

        private static void WriteCommandBefore(CommandEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlBeforeExecuteCommand))
            {
                _diagnosticListener.Write(
                    SqlBeforeExecuteCommand,
                    new
                    {
                        OperationId = e.CommandId,
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Command = e.Command
                    });
            }
        }

        private static void WriteCommandAfter(CommandExecutedEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlAfterExecuteCommand))
            {
                _diagnosticListener.Write(
                    SqlAfterExecuteCommand,
                    new
                    {
                        OperationId = e.CommandId,
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Command = e.Command,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteCommandError(CommandErrorEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlErrorExecuteCommand))
            {
                _diagnosticListener.Write(
                    SqlErrorExecuteCommand,
                    new
                    {
                        OperationId = e.CommandId,
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Command = e.Command,
                        Exception = e.Exception,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteConnectionOpenBefore(ConnectionEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlBeforeOpenConnection))
            {
                _diagnosticListener.Write(
                    SqlBeforeOpenConnection,
                    new
                    {
                        OperationId = e.ConnectionId,
                        Operation = operation,
                        Connection = e.Connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteConnectionOpenAfter(ConnectionEndEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlAfterOpenConnection))
            {
                _diagnosticListener.Write(
                    SqlAfterOpenConnection,
                    new
                    {
                        OperationId = e.ConnectionId,
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Connection = e.Connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteConnectionOpenError(ConnectionErrorEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlErrorOpenConnection))
            {
                _diagnosticListener.Write(
                    SqlErrorOpenConnection,
                    new
                    {
                        OperationId = e.ConnectionId,
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Connection = e.Connection,
                        Exception = e.Exception,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteConnectionCloseBefore(ConnectionEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlBeforeCloseConnection))
            {
                _diagnosticListener.Write(
                    SqlBeforeCloseConnection,
                    new
                    {
                        OperationId = CreateComposedGuid(e.ConnectionId),
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Connection = e.Connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteConnectionCloseAfter(ConnectionEndEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlAfterCloseConnection))
            {
                _diagnosticListener.Write(
                    SqlAfterCloseConnection,
                    new
                    {
                        OperationId = CreateComposedGuid(e.ConnectionId),
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Connection = e.Connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteConnectionCloseError(ConnectionErrorEventData e, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlErrorCloseConnection))
            {
                _diagnosticListener.Write(
                    SqlErrorCloseConnection,
                    new
                    {
                        OperationId = CreateComposedGuid(e.ConnectionId),
                        Operation = operation,
                        ConnectionId = e.ConnectionId,
                        Connection = e.Connection,
                        Exception = e.Exception,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteTransactionCommitBefore(TransactionEventData e, DbTransactionInformation e2, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlBeforeCommitTransaction))
            {
                _diagnosticListener.Write(
                    SqlBeforeCommitTransaction,
                    new
                    {
                        OperationId = e2.CorrelationId,
                        Operation = operation,
                        IsolationLevel = e.Transaction.IsolationLevel,
                        Connection = e.Transaction.Connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteTransactionCommitAfter(TransactionEndEventData _, DbTransactionInformation e2, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlAfterCommitTransaction))
            {
                _diagnosticListener.Write(
                    SqlAfterCommitTransaction,
                    new
                    {
                        OperationId = e2.CorrelationId,
                        Operation = operation,
                        IsolationLevel = e2.IsolationLevel,
                        Connection = e2.Connection,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteTransactionCommitError(TransactionErrorEventData e, DbTransactionInformation e2, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlErrorCommitTransaction))
            {
                _diagnosticListener.Write(
                    SqlErrorCommitTransaction,
                    new
                    {
                        OperationId = e2.CorrelationId,
                        Operation = operation,
                        IsolationLevel = e2.IsolationLevel,
                        Connection = e2.Connection,
                        Exception = e.Exception,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteTransactionRollbackBefore(TransactionEventData e, DbTransactionInformation e2, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlBeforeRollbackTransaction))
            {
                _diagnosticListener.Write(
                    SqlBeforeRollbackTransaction,
                    new
                    {
                        OperationId = e2.CorrelationId,
                        Operation = operation,
                        IsolationLevel = e.Transaction.IsolationLevel,
                        Connection = e.Transaction.Connection,
                        TransactionName = e.TransactionId.ToString(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteTransactionRollbackAfter(TransactionEndEventData e, DbTransactionInformation e2, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlAfterRollbackTransaction))
            {
                _diagnosticListener.Write(
                    SqlAfterRollbackTransaction,
                    new
                    {
                        OperationId = e2.CorrelationId,
                        Operation = operation,
                        IsolationLevel = e2.IsolationLevel,
                        Connection = e2.Connection,
                        TransactionName = e.TransactionId.ToString(),
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        private static void WriteTransactionRollbackError(TransactionErrorEventData e, DbTransactionInformation e2, [CallerMemberName] string operation = "")
        {
            if (_diagnosticListener.IsEnabled(SqlErrorRollbackTransaction))
            {
                _diagnosticListener.Write(
                    SqlErrorRollbackTransaction,
                    new
                    {
                        OperationId = e2.CorrelationId,
                        Operation = operation,
                        IsolationLevel = e2.IsolationLevel,
                        Connection = e2.Connection,
                        TransactionName = e.TransactionId.ToString(),
                        Exception = e.Exception,
                        Timestamp = Stopwatch.GetTimestamp()
                    });
            }
        }

        #endregion

        #region CommandInterceptor

        private class DbTransactionInformation
        {
            public IsolationLevel IsolationLevel { get; }

            public DbConnection? Connection { get; }

            public Guid CorrelationId { get; }

            public DbTransactionInformation(DbTransaction transaction)
            {
                IsolationLevel = transaction.IsolationLevel;
                Connection = transaction.Connection;
                CorrelationId = Guid.NewGuid();
            }
        }

        private readonly ConditionalWeakTable<DbTransaction, DbTransactionInformation> transactionInfo
            = new ConditionalWeakTable<DbTransaction, DbTransactionInformation>();

        public DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
        {
            return result;
        }

        public InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData, InterceptionResult<DbCommand> result)
        {
            return result;
        }

        public void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            WriteCommandError(eventData);
        }

        public Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            WriteCommandError(eventData);
            return Task.CompletedTask;
        }

        public void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
        {
            WriteConnectionCloseAfter(eventData);
        }

        public Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
        {
            WriteConnectionCloseAfter(eventData);
            return Task.CompletedTask;
        }

        public InterceptionResult ConnectionClosing(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            WriteConnectionCloseBefore(eventData);
            return result;
        }

        public ValueTask<InterceptionResult> ConnectionClosingAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            WriteConnectionCloseBefore(eventData);
            return ValueTask.FromResult(result);
        }

        public void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData)
        {
            switch (eventData.Connection.State)
            {
                case ConnectionState.Broken:
                case ConnectionState.Open:
                    WriteConnectionCloseError(eventData);
                    break;

                case ConnectionState.Closed:
                case ConnectionState.Connecting:
                    WriteConnectionOpenError(eventData);
                    break;

                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                default:
                    break;
            }
        }

        public Task ConnectionFailedAsync(DbConnection connection, ConnectionErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            switch (eventData.Connection.State)
            {
                case ConnectionState.Broken:
                case ConnectionState.Open:
                    WriteConnectionCloseError(eventData);
                    break;

                case ConnectionState.Closed:
                case ConnectionState.Connecting:
                    WriteConnectionOpenError(eventData);
                    break;

                case ConnectionState.Executing:
                case ConnectionState.Fetching:
                default:
                    break;
            }

            return Task.CompletedTask;
        }

        public void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            WriteConnectionOpenAfter(eventData);
        }

        public Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            WriteConnectionOpenAfter(eventData);
            return Task.CompletedTask;
        }

        public InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            WriteConnectionOpenBefore(eventData);
            return result;
        }

        public ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            WriteConnectionOpenBefore(eventData);
            return ValueTask.FromResult(result);
        }

        public InterceptionResult DataReaderDisposing(DbCommand command, DataReaderDisposingEventData eventData, InterceptionResult result)
        {
            return result;
        }

        public int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
        {
            WriteCommandAfter(eventData);
            return result;
        }

        public ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            WriteCommandAfter(eventData);
            return ValueTask.FromResult(result);
        }

        public InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            WriteCommandBefore(eventData);
            return result;
        }

        public ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            WriteCommandBefore(eventData);
            return ValueTask.FromResult(result);
        }

        public DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            WriteCommandAfter(eventData);
            return result;
        }

        public ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            WriteCommandAfter(eventData);
            return ValueTask.FromResult(result);
        }

        public InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            WriteCommandBefore(eventData);
            return result;
        }

        public ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            WriteCommandBefore(eventData);
            return ValueTask.FromResult(result);
        }

        public object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
        {
            WriteCommandAfter(eventData);
            return result;
        }

        public ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
        {
            WriteCommandAfter(eventData);
            return ValueTask.FromResult(result);
        }

        public InterceptionResult<object> ScalarExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            WriteCommandBefore(eventData);
            return result;
        }

        public ValueTask<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            WriteCommandBefore(eventData);
            return ValueTask.FromResult(result);
        }

        public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
        {
            if (transactionInfo.TryGetValue(transaction, out var info))
            {
                WriteTransactionCommitAfter(eventData, info);
                transactionInfo.Remove(transaction);
            }
        }

        public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (transactionInfo.TryGetValue(transaction, out var info))
            {
                WriteTransactionCommitAfter(eventData, info);
                transactionInfo.Remove(transaction);
            }

            return Task.CompletedTask;
        }

        public InterceptionResult TransactionCommitting(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        {
            var info = new DbTransactionInformation(transaction);
            WriteTransactionCommitBefore(eventData, info);
            transactionInfo.Add(transaction, info);
            return result;
        }

        public ValueTask<InterceptionResult> TransactionCommittingAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            var info = new DbTransactionInformation(transaction);
            WriteTransactionCommitBefore(eventData, info);
            transactionInfo.Add(transaction, info);
            return ValueTask.FromResult(result);
        }

        public void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
        {
            if (transactionInfo.TryGetValue(transaction, out var info))
            {
                if (eventData.Action == "Commit")
                {
                    WriteTransactionCommitError(eventData, info);
                }
                else
                {
                    WriteTransactionRollbackError(eventData, info);
                }

                transactionInfo.Remove(transaction);
            }
        }

        public Task TransactionFailedAsync(DbTransaction transaction, TransactionErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            if (transactionInfo.TryGetValue(transaction, out var info))
            {
                if (eventData.Action == "Commit")
                {
                    WriteTransactionCommitError(eventData, info);
                }
                else
                {
                    WriteTransactionRollbackError(eventData, info);
                }

                transactionInfo.Remove(transaction);
            }

            return Task.CompletedTask;
        }

        public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
        {
            if (transactionInfo.TryGetValue(transaction, out var info))
            {
                WriteTransactionRollbackAfter(eventData, info);
                transactionInfo.Remove(transaction);
            }
        }

        public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (transactionInfo.TryGetValue(transaction, out var info))
            {
                WriteTransactionRollbackAfter(eventData, info);
                transactionInfo.Remove(transaction);
            }

            return Task.CompletedTask;
        }

        public InterceptionResult TransactionRollingBack(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        {
            var info = new DbTransactionInformation(transaction);
            WriteTransactionRollbackBefore(eventData, info);
            transactionInfo.Add(transaction, info);
            return result;
        }

        public ValueTask<InterceptionResult> TransactionRollingBackAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            var info = new DbTransactionInformation(transaction);
            WriteTransactionRollbackBefore(eventData, info);
            transactionInfo.Add(transaction, info);
            return ValueTask.FromResult(result);
        }

        public DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
        {
            return result;
        }

        public ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        public InterceptionResult<DbTransaction> TransactionStarting(DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result)
        {
            return result;
        }

        public ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(DbConnection connection, TransactionStartingEventData eventData, InterceptionResult<DbTransaction> result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        public DbTransaction TransactionUsed(DbConnection connection, TransactionEventData eventData, DbTransaction result)
        {
            return result;
        }

        public ValueTask<DbTransaction> TransactionUsedAsync(DbConnection connection, TransactionEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        public InterceptionResult CreatingSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        {
            return result;
        }

        public void CreatedSavepoint(DbTransaction transaction, TransactionEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> CreatingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        public Task CreatedSavepointAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public InterceptionResult RollingBackToSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        {
            return result;
        }

        public void RolledBackToSavepoint(DbTransaction transaction, TransactionEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> RollingBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        public Task RolledBackToSavepointAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public InterceptionResult ReleasingSavepoint(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result)
        {
            return result;
        }

        public void ReleasedSavepoint(DbTransaction transaction, TransactionEventData eventData)
        {
        }

        public ValueTask<InterceptionResult> ReleasingSavepointAsync(DbTransaction transaction, TransactionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(result);
        }

        public Task ReleasedSavepointAsync(DbTransaction transaction, TransactionEventData eventData, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
