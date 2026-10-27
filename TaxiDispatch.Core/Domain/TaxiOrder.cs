namespace TaxiDispatch.Core.Domain
{
    /// <summary>
    /// Представляє замовлення таксі в системі.
    /// </summary>
    public class TaxiOrder
    {
        /// <summary>Ставка податку на додану вартість.</summary>
        private const decimal VatRate = 0.2m;
        /// <summary>Поріг суми для знижки постійному пасажиру.</summary>
        private const decimal RegularDiscountThreshold = 1000m;
        /// <summary>Частка знижки постійному пасажиру.</summary>
        private const decimal RegularDiscountRate = 0.10m;
        /// <summary>Поріг суми для знижки на велике замовлення.</summary>
        private const decimal LargeOrderThreshold = 5000m;
        /// <summary>Частка знижки на велике замовлення.</summary>
        private const decimal LargeOrderDiscountRate = 0.15m;
        /// <summary>Кількість відрізків, з якої діє гуртова знижка.</summary>
        private const int BulkSegmentCount = 10;
        /// <summary>Сума гуртової знижки, грн.</summary>
        private const decimal BulkDiscountAmount = 100m;
        /// <summary>Максимальна кількість відрізків у замовленні.</summary>
        private const int MaxSegmentCount = 100;
        /// <summary>Мінімальна довжина імені пасажира.</summary>
        private const int MinPassengerNameLength = 2;

        private const int StatusNew = 0;
        private const int StatusAssigned = 1;
        private const int StatusInProgress = 2;
        private const int StatusCancelled = 3;

        private readonly System.Collections.Generic.List<string[]> _segments = new System.Collections.Generic.List<string[]>();

        /// <summary>
        /// Ініціалізує нове замовлення таксі.
        /// </summary>
        /// <param name="id">Унікальний ідентифікатор замовлення.</param>
        /// <param name="passengerName">Ім'я пасажира.</param>
        public TaxiOrder(string id, string passengerName)
        {
            Id = id;
            PassengerName = passengerName;
            CreatedAt = System.DateTime.Now;
        }

        /// <summary>
        /// Отримує ідентифікатор замовлення.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Отримує ім'я пасажира.
        /// </summary>
        public string PassengerName { get; private set; }

        /// <summary>
        /// Отримує поточний статус замовлення.
        /// </summary>
        public int Status { get; private set; } = StatusNew;

        /// <summary>
        /// Отримує дату та час створення замовлення.
        /// </summary>
        public System.DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Шукає замовлення за ідентифікатором у списку.
        /// </summary>
        /// <param name="orders">Список замовлень для пошуку.</param>
        /// <param name="orderId">Ідентифікатор шуканого замовлення.</param>
        /// <returns>Знайдене замовлення або null, якщо не знайдено.</returns>
        public static TaxiOrder? FindById(System.Collections.Generic.List<TaxiOrder> orders, string orderId)
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
        /// Додає відрізок маршруту до замовлення.
        /// </summary>
        /// <param name="destination">Кінцевий пункт відрізка.</param>
        /// <param name="distance">Дистанція відрізка.</param>
        /// <param name="price">Ціна за відрізок.</param>
        public void AddSegment(string destination, int distance, decimal price)
        {
            string[] t = new string[3];
            t[0] = destination;
            t[1] = distance.ToString();
            t[2] = price.ToString();
            _segments.Add(t);
        }

        /// <summary>
        /// Обчислює підсумкову суму замовлення з урахуванням знижок і податків.
        /// </summary>
        /// <param name="isRegularPassenger">Ознака постійного пасажира.</param>
        /// <returns>Підсумкова сума до сплати.</returns>
        public decimal CalculateTotal(bool isRegularPassenger)
        {
            decimal total = 0;
            int segmentCount = 0;

            for (int i = 0; i < _segments.Count; i++)
            {
                int k = int.Parse(_segments[i][1]);
                decimal c = decimal.Parse(_segments[i][2]);
                total += k * c;
                segmentCount++;
            }

            if (isRegularPassenger && total > RegularDiscountThreshold)
            {
                total *= 1m - RegularDiscountRate;
            }
            else if (total > LargeOrderThreshold)
            {
                total *= 1m - LargeOrderDiscountRate;
            }

            if (segmentCount > BulkSegmentCount)
            {
                total -= BulkDiscountAmount;
            }

            if (total < 0)
            {
                total = 0;
            }

            total += total * VatRate;
            return System.Math.Round(total, 2);
        }

        /// <summary>
        /// Намагається змінити статус замовлення.
        /// </summary>
        /// <param name="newStatus">Новий статус для встановлення.</param>
        /// <returns>true, якщо статус успішно змінено; false, якщо перехід заборонено.</returns>
        public bool TryChangeStatus(int newStatus)
        {
            if (Status == StatusNew && newStatus == StatusAssigned)
            {
                Status = StatusAssigned;
                return true;
            }

            if (Status == StatusAssigned && newStatus == StatusInProgress)
            {
                Status = StatusInProgress;
                return true;
            }

            if (Status == StatusNew && newStatus == StatusCancelled)
            {
                Status = StatusCancelled;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Перевіряє валідність замовлення.
        /// </summary>
        /// <returns>true, якщо замовлення валідне; інакше false.</returns>
        public bool IsValid()
        {
            if (Id != null && Id != string.Empty
                && PassengerName != null
                && PassengerName.Length > MinPassengerNameLength
                && _segments.Count > 0
                && _segments.Count < MaxSegmentCount
                && Status >= StatusNew
                && Status <= StatusCancelled)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Формує текстовий звіт про замовлення.
        /// </summary>
        /// <returns>Рядок із деталями замовлення.</returns>
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
}
