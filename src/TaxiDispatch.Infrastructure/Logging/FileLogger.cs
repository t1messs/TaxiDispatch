using Microsoft.Extensions.Logging;

namespace TaxiDispatch.Infrastructure.Logging;

/// <summary>
/// Журнал однієї категорії; усі категорії пишуть в один файл через
/// <see cref="FileLoggerProvider"/>.
/// </summary>
internal sealed class FileLogger : ILogger
{
    private readonly FileLoggerProvider _provider;

    public FileLogger(FileLoggerProvider provider) => _provider = provider;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => _provider.PushScope(state.ToString() ?? string.Empty);

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            _provider.Write(logLevel, formatter(state, exception), exception);
        }
    }
}
