using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiDispatch.Core.Domain
{
    // клас
    public class TaxiOrder
    {
        public string Id { get; private set; }
        public string PassengerName { get; private set; }
        private readonly List<string[]> _segments = new List<string[]>();
        public int Status { get; private set; } = 0; // 0-новий,1-признач,2-в_дороз,3-скасов
        public DateTime CreatedAt { get; private set; }
        // public string prim; // примітка, поки не треба

        // конструктор
        public TaxiOrder(string id, string passengerName)
        {
            Id = id; // ставимо id
            PassengerName = passengerName; // ставимо pass
            CreatedAt = DateTime.Now; // ставимо дату
        }

        // метод додавання
        public void AddSegment(string destination, int distance, decimal price)
        {
            string[] t = new string[3];
            t[0] = destination; t[1] = distance.ToString(); t[2] = price.ToString();
            _segments.Add(t); // додаємо t у sg
        }

        // ProcessData
        public decimal CalculateTotal(bool isRegularPassenger)
        {
            decimal total = 0; int segmentCount = 0;
            for (int i = 0; i < _segments.Count; i++)
            {
                int k = int.Parse(_segments[i][1]);
                decimal c = decimal.Parse(_segments[i][2]);
                total = total + k * c; // додаємо до суми
                segmentCount = segmentCount + 1; // збільшуємо kolvo на одиницю
            }
            // if (total > 500) { total = total - 50; } // стара знижка

            if (isRegularPassenger == true && total > 1000) { total = total * 0.9m; } else if (total > 5000) { total = total * 0.85m; } else { total = total; }

            if (segmentCount > 10) total = total - 100;
            if (total < 0) total = 0;
            total = total + total * 0.2m;
            return Math.Round(total, 2); // повертаємо sum1
        }

        // міняємо статус
        public bool TryChangeStatus(int newStatus)
        {
            if (Status == 0 && newStatus == 1) { Status = 1; return true; }
            if (Status == 1 && newStatus == 2) { Status = 2; return true; }
            if (Status == 0 && newStatus == 3) { Status = 3; return true; }
            return false; // не можна
        }

        // перевірка
        public bool IsValid()
        {
            if (Id != null && Id != "" && PassengerName != null && PassengerName.Length > 2 && _segments.Count > 0 && _segments.Count < 100 && Status >= 0 && Status <= 3) return true;
            else
                return false;
        }

        // звіт
        public string BuildReport()
        {
            string s = "";
            for (int i = 0; i < _segments.Count; i++)
            {
                s = s + "Відрізок: " + _segments[i][0] + "; дистанція: " + _segments[i][1] + "; ціна: " + _segments[i][2] + "; сума: " + (int.Parse(_segments[i][1]) * decimal.Parse(_segments[i][2])) + "\n";
            }
            s = s + "Разом: " + CalculateTotal(false) + "\n";
            return s; // повертаємо s
        }

        // пошук
        public static TaxiOrder FindById(List<TaxiOrder> orders, string orderId)
        {
            for (int i = 0; i < orders.Count; i++)
            {
                if (orders[i].Id == orderId) { return orders[i]; }
            }
            return null;
        }
    }
}
