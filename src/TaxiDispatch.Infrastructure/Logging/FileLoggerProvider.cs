using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;

namespace TaxiDispatch.Infrastructure.Logging;

/// <summary>
/// Журнал у файл: один рядок на запис — час, рівень, сценарій (область журналу), повідомлення;
/// для винятку — його тип і текст. Персональних даних сценарії до журналу не передають.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private const string NoScope = "-";

    private readonly StreamWriter _writer;
    private readonly TimeProvider _clock;
    private readonly AsyncLocal<string?> _scope = new();
    private readonly Lock _gate = new();

    /// <summary>
    /// Відкриває файл журналу для дописування; теку створює за потреби.
    /// </summary>
    /// <param name="path">Шлях до файлу журналу.</param>
    /// <param name="clock">Джерело часу для позначок у журналі.</param>
    public FileLoggerProvider(string path, TimeProvider clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        string? directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (directory is not null)
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(path, append: true, Encoding.UTF8) { AutoFlush = true };
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName) => new FileLogger(this);

    /// <inheritdoc/>
    public void Dispose() => _writer.Dispose();

    /// <summary>Відкриває область журналу (номер сценарію) до виклику Dispose.</summary>
    internal IDisposable PushScope(string scope)
    {
        string? previous = _scope.Value;
        _scope.Value = scope;
        return new ScopeExit(this, previous);
    }

    /// <summary>Пише один рядок журналу.</summary>
    internal void Write(LogLevel level, string message, Exception? exception)
    {
        CultureInfo invariant = CultureInfo.InvariantCulture;
        var line = new StringBuilder();
        line.Append(invariant, $"{_clock.GetLocalNow():yyyy-MM-dd HH:mm:ss zzz} {level,-11} ");
        line.Append(invariant, $"[{_scope.Value ?? NoScope}] {message}");
        if (exception is not null)
        {
            line.Append(invariant, $" | {exception.GetType().Name}: {exception.Message}");
        }

        lock (_gate)
        {
            _writer.WriteLine(line.ToString());
        }
    }

    private sealed class ScopeExit : IDisposable
    {
        private readonly FileLoggerProvider _owner;
        private readonly string? _previous;

        public ScopeExit(FileLoggerProvider owner, string? previous)
        {
            _owner = owner;
            _previous = previous;
        }

        public void Dispose() => _owner._scope.Value = _previous;
    }
}
