using System.Text;
using TaxiDispatch.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var z = new TaxiOrder("TX-1001", "Іваненко");
z.AddSegment("Вулиця 1 - Вулиця 2", 3, 250m);
z.AddSegment("Вулиця 2 - Вулиця 3", 12, 90m);

Console.WriteLine(z.CalculateTotal(false));
Console.WriteLine(z.CalculateTotal(true));
Console.WriteLine(z.IsValid());
Console.WriteLine(z.TryChangeStatus(1));
Console.Write(z.BuildReport());
