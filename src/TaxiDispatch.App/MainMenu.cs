using TaxiDispatch.Domain.Errors;

namespace TaxiDispatch.App;

/// <summary>
/// Головне меню: показує пункти, викликає екран сценарію й перекладає порушення правил
/// домену зрозумілим повідомленням. Розрахунків і правил тут немає.
/// </summary>
internal sealed class MainMenu
{
    private const string ExitChoice = "0";

    private readonly ConsoleIo _io;
    private readonly Dictionary<string, (string Title, Action Run)> _items;

    public MainMenu(ConsoleIo io, OrderScreens orders, ReportScreens reports)
    {
        _io = io;
        _items = new Dictionary<string, (string Title, Action Run)>(StringComparer.Ordinal)
        {
            ["1"] = ("Прийняти замовлення (UC-01)", orders.Accept),
            ["2"] = ("Призначити водія (UC-02)", orders.Assign),
            ["3"] = ("Почати поїздку — пасажир сів (UC-03)", orders.Start),
            ["4"] = ("Завершити поїздку (UC-03)", orders.Complete),
            ["5"] = ("Скасувати замовлення (UC-04)", orders.Cancel),
            ["6"] = ("Знайти замовлення (UC-05)", reports.Find),
            ["7"] = ("Звіт «Поїздки за зміну» (UC-06)", reports.ShiftReport),
            ["8"] = ("Довідник місць", reports.Places),
        };
    }

    public void Run()
    {
        while (true)
        {
            _io.Say(string.Empty);
            foreach ((string key, (string title, _)) in _items)
            {
                _io.Say($"{key} — {title}");
            }

            _io.Say($"{ExitChoice} — Вихід");
            string choice = _io.Read("Ваш вибір").Trim();
            if (choice == ExitChoice)
            {
                return;
            }

            RunItem(choice);
        }
    }

    private void RunItem(string choice)
    {
        if (!_items.TryGetValue(choice, out (string Title, Action Run) item))
        {
            _io.Say("  ! Немає такого пункту меню.");
            return;
        }

        try
        {
            item.Run();
        }
        catch (DomainRuleException error)
        {
            // Сервіс уже записав Warning до журналу; оператору — пояснення, робота триває.
            _io.Say($"  Відхилено: {error.Message}");
        }
    }
}
