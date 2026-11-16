using System.Globalization;

namespace TaxiDispatch.App;

/// <summary>
/// Журнал аудиту у файлі: дописує рядок «час;операція;документ».
/// Клас володіє відкритим файлом, тому сам реалізує IDisposable.
/// </summary>
internal sealed class FileAuditLog : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>
    /// Відкриває (або створює) файл аудиту для дописування.
    /// </summary>
    /// <param name="path">Шлях до файлу журналу аудиту.</param>
    public FileAuditLog(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _writer = new StreamWriter(path, append: true);
    }

    /// <summary>
    /// Дописує рядок аудиту.
    /// </summary>
    /// <param name="operation">Назва операції.</param>
    /// <param name="documentId">Номер документа (замовлення).</param>
    public void Write(string operation, int documentId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _writer.WriteLine(string.Create(
            CultureInfo.InvariantCulture,
            $"{DateTimeOffset.Now:O};{operation};{documentId}"));
    }

    /// <summary>
    /// Скидає буфер на диск і закриває файл; повторний виклик нічого не робить.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _writer.Flush();
        _writer.Dispose();
        _disposed = true;
    }
}
