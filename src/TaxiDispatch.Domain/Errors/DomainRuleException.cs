namespace TaxiDispatch.Domain.Errors;

/// <summary>
/// Порушено правило предметної області: дія користувача суперечить бізнес-правилу.
/// </summary>
public class DomainRuleException : Exception
{
    /// <summary>
    /// Створює виняток із ідентифікатором порушеного правила.
    /// </summary>
    /// <param name="rule">Ідентифікатор правила, наприклад "order.editable".</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    public DomainRuleException(string rule, string message)
        : base(message)
    {
        Rule = rule;
    }

    /// <summary>
    /// Створює виняток із ідентифікатором правила та внутрішнім винятком-причиною.
    /// </summary>
    /// <param name="rule">Ідентифікатор правила.</param>
    /// <param name="message">Повідомлення українською з конкретними значеннями.</param>
    /// <param name="inner">Виняток, що спричинив порушення.</param>
    public DomainRuleException(string rule, string message, Exception inner)
        : base(message, inner)
    {
        Rule = rule;
    }

    /// <summary>Отримує ідентифікатор порушеного правила, наприклад "order.editable".</summary>
    public string Rule { get; }
}
