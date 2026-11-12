using TaxiDispatch.Core.Abstractions;

namespace TaxiDispatch.App;

/// <summary>
/// Сповіщувач для консольного застосунку: замість SMS виводить текст у консоль.
/// </summary>
internal sealed class ConsoleNotifier : INotifier
{
    /// <inheritdoc/>
    public void Notify(string recipient, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipient);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Console.WriteLine($"Сповіщення для {recipient}: {message}");
    }
}
