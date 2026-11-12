namespace TaxiDispatch.Core.Abstractions;

/// <summary>
/// Сповіщувач: вузький інтерфейс з одним методом надсилання повідомлення.
/// </summary>
public interface INotifier
{
    /// <summary>Надсилає повідомлення одержувачу.</summary>
    /// <param name="recipient">Одержувач (наприклад, телефон пасажира).</param>
    /// <param name="message">Текст повідомлення.</param>
    void Notify(string recipient, string message);
}
