using System.Text;
using TaxiDispatch.Core.Domain;

Console.OutputEncoding = Encoding.UTF8;

var z = new zakaz("TX-1001", "Іваненко");
z.Add("Вулиця 1 - Вулиця 2", 3, 250m);
z.Add("Вулиця 2 - Вулиця 3", 12, 90m);

Console.WriteLine(z.ProcessData(false));
Console.WriteLine(z.ProcessData(true));
Console.WriteLine(z.Ck());
Console.WriteLine(z.Chg(1));
Console.Write(z.Rep());
