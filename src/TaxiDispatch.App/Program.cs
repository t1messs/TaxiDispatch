using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using TaxiDispatch.App;
using TaxiDispatch.Infrastructure.Logging;

// Верхній рівень: культура, кодування, журнал і єдиний обробник непередбачених збоїв.
CultureInfo ukrainian = CultureInfo.GetCultureInfo("uk-UA");
CultureInfo.DefaultThreadCurrentCulture = ukrainian;
CultureInfo.CurrentCulture = ukrainian;
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;
var io = new ConsoleIo(Console.In, Console.Out);
io.Say($"TaxiDispatch v{VersionInfo.Current()} — диспетчерська служби таксі");

TimeProvider clock = TimeProvider.System;
using var logFile = new FileLoggerProvider(Path.Combine("logs", "taxidispatch.log"), clock);
using ILoggerFactory logs = LoggerFactory.Create(builder => builder.AddProvider(logFile));
ILogger log = logs.CreateLogger("TaxiDispatch.App");
try
{
    Composition.CreateMenu(io, logs, clock).Run();
    return 0;
}
catch (EndOfStreamException)
{
    log.LogInformation("Ввід завершено, роботу закінчено");
    return 0;
}
catch (InvalidDataException error)
{
    log.LogError(error, "Не вдалося завантажити дані");
    io.Say($"Дані пошкоджено: {error.Message}");
    return 2;
}
catch (IOException error)
{
    log.LogError(error, "Збій файлової системи");
    io.Say("Не вдалося прочитати чи записати файли в data/ або state/. Подробиці — у журналі.");
    return 3;
}
catch (Exception error)
{
    // Помилка програміста: падаємо гучно, але з записом до журналу.
    log.LogCritical(error, "Непередбачений збій");
    io.Say("Непередбачений збій. Подробиці — у logs/taxidispatch.log.");
    return 1;
}
