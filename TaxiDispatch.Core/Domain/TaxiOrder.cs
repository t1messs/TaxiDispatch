using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiDispatch.Core.Domain;

/// <summary>
/// Представляє замовлення таксі в системі.
/// </summary>
public class TaxiOrder
{
    private readonly List<string[]> _segments = new List<string[]>();

    /// <summary>
    /// Ініціалізує нове замовлення таксі.
    /// </summary>
    /// <param name="id">Унікальний ідентифікатор замовлення.</param>
    /// <param name="passengerName">Ім'я пасажира.</param>
    public TaxiOrder(string id, string passengerName)
    {
        Id = id;
        PassengerName = passengerName;
        CreatedAt = DateTime.Now;
    }

    /// <summary>Отримує ідентифікатор замовлення.</summary>
    public string Id { get; private set; }

    /// <summary>Отримує ім'я пасажира.</summary>
    public string PassengerName { get; private set; }

    /// <summary>
    /// Отримує поточний стан замовлення:
    /// 0 — нове, 1 — призначено, 2 — у дорозі, 3 — скасовано.
    /// </summary>
    public int Status { get; private set; } = 0;

    /// <summary>Отримує дату й час створення замовлення.</summary>
    public DateTime CreatedAt { get; private set; }

    // public string prim; // примітка, поки не треба

    /// <summary>
    /// Шукає замовлення за ідентифікатором у списку.
    /// </summary>
    /// <param name="orders">Список замовлень, у якому виконується пошук.</param>
    /// <param name="orderId">Ідентифікатор шуканого замовлення.</param>
    /// <returns>Знайдене замовлення або null, якщо такого замовлення немає.</returns>
    public static TaxiOrder FindById(List<TaxiOrder> orders, string orderId)
    {
        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i].Id == orderId)
            {
                return orders[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Додає до замовлення відрізок маршруту.
    /// </summary>
    /// <param name="destination">Адреса кінцевого пункту відрізка.</param>
    /// <param name="distance">Дистанція відрізка в кілометрах.</param>
    /// <param name="price">Ціна за один кілометр.</param>
    public void AddSegment(string destination, int distance, decimal price)
    {
        string[] t = new string[3];
        t[0] = destination;
        t[1] = distance.ToString();
        t[2] = price.ToString();
        _segments.Add(t);
    }

    /// <summary>
    /// Обчислює підсумкову суму замовлення з урахуванням знижок і ПДВ.
    /// </summary>
    /// <param name="isRegularPassenger">Ознака постійного пасажира.</param>
    /// <returns>Сума до сплати, заокруглена до двох знаків.</returns>
    public decimal CalculateTotal(bool isRegularPassenger)
    {
        decimal total = 0;
        int segmentCount = 0;

        for (int i = 0; i < _segments.Count; i++)
        {
            int k = int.Parse(_segments[i][1]);
            decimal c = decimal.Parse(_segments[i][2]);
            total = total + (k * c);
            segmentCount = segmentCount + 1;
        }

        // if (total > 500) { total = total - 50; } // стара знижка
        // Знижки не сумуються: діє лише перша підходяща (спершу — постійному пасажиру)
        if (isRegularPassenger == true && total > 1000)
        {
            total = total * 0.9m;
        }
        else if (total > 5000)
        {
            total = total * 0.85m;
        }
        else
        {
            total = total;
        }

        if (segmentCount > 10)
        {
            total = total - 100;
        }

        if (total < 0)
        {
            total = 0;
        }

        // ПДВ нараховується на суму вже після всіх знижок
        total = total + (total * 0.2m);
        return Math.Round(total, 2);
    }

    /// <summary>
    /// Змінює стан замовлення, якщо перехід дозволений правилами предметної області.
    /// </summary>
    /// <param name="newStatus">Цільовий стан замовлення.</param>
    /// <returns>true, якщо перехід виконано; false, якщо він заборонений.</returns>
    public bool TryChangeStatus(int newStatus)
    {
        if (Status == 0 && newStatus == 1)
        {
            Status = 1;
            return true;
        }

        if (Status == 1 && newStatus == 2)
        {
            Status = 2;
            return true;
        }

        if (Status == 0 && newStatus == 3)
        {
            Status = 3;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Перевіряє коректність замовлення.
    /// </summary>
    /// <returns>true, якщо замовлення коректне; інакше false.</returns>
    public bool IsValid()
    {
        if (Id != null && Id != string.Empty
            && PassengerName != null
            && PassengerName.Length > 2
            && _segments.Count > 0
            && _segments.Count < 100
            && Status >= 0
            && Status <= 3)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Формує текстовий звіт про замовлення.
    /// </summary>
    /// <returns>Рядок з описом кожного відрізка маршруту та підсумковою сумою.</returns>
    public string BuildReport()
    {
        string s = string.Empty;

        for (int i = 0; i < _segments.Count; i++)
        {
            s = s + "Відрізок: " + _segments[i][0]
                + "; дистанція: " + _segments[i][1]
                + "; ціна: " + _segments[i][2]
                + "; сума: " + (int.Parse(_segments[i][1]) * decimal.Parse(_segments[i][2]))
                + "\n";
        }

        s = s + "Разом: " + CalculateTotal(false) + "\n";
        return s;
    }
}