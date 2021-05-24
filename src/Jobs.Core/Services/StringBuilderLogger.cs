using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace Jobs.Services
{
    public class StringBuilderLogger : ILogger
    {
        public StringBuilder StringBuilder { get; }

        public StringBuilderLogger()
            => StringBuilder = new StringBuilder();

        public IDisposable BeginScope<TState>(TState state)
            => new DeferredEndScope<TState>(StringBuilder, state);

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => StringBuilder
                .AppendLine($"[{DateTimeOffset.Now:yyyy/M/d HH:mm:ss zzz}] {logLevel} ({eventId})")
                .AppendLine(formatter(state, exception));

        private class DeferredEndScope<TState> : IDisposable
        {
            private readonly StringBuilder _sb;
            private readonly TState _state;

            public DeferredEndScope(StringBuilder sb, TState state)
                => (_sb, _state) = (sb, state);

            public void Dispose()
                => _sb.AppendLine($"[{DateTimeOffset.Now:yyyy/M/d HH:mm:ss zzz}]")
                    .AppendLine($"<===== END OF SCOPE \"{_state}\" =====>");
        }
    }
}
