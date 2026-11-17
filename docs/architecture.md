# Архітектура TaxiDispatch

## 1. Шари

| Шар | Проєкт | Відповідальність (одне речення) | Що містить | Що йому заборонено |
|---|---|---|---|---|
| Домен | `TaxiDispatch.Domain` | Правила предметної області: як рахується вартість, кого призначати й які переходи стану дозволені. | сутності (`TaxiOrder`, `Driver`, `Passenger`, `Place`), перелічення станів, `FareCalculator`, `DriverMatcher`, `OrderTransitions`, `InvalidOrderStateException` | знати про файли, консоль, JSON, журнал, інші шари |
| Прикладний | `TaxiDispatch.Application` | Сценарії UC-01…UC-06 від перевірки вводу до збереження результату. | сервіси сценаріїв, барикада `InputParser`, інтерфейси `ITaxiStore` і `IFarePolicy` | знати формат зберігання й консоль |
| Інфраструктура | `TaxiDispatch.Infrastructure` | Технічні деталі: файли JSON і журнал у файл. | `JsonTaxiStore`, `JsonTariffSource`, `FileLoggerProvider` | містити правила домену |
| Застосунок | `TaxiDispatch.App` | Розмова з оператором і з'єднання шарів. | меню, екрани, форматування виводу, композиційний корінь | містити розрахунки й правила |

## 2. Правило залежностей

```
TaxiDispatch.App ──────────► TaxiDispatch.Application ──────────► TaxiDispatch.Domain
        │                               ▲
        └──────► TaxiDispatch.Infrastructure ┘
```

- **App → Application.** Меню викликає сервіси сценаріїв і барикаду вводу, але не знає, як
  рахується вартість.
- **App → Infrastructure.** Лише композиційний корінь створює `JsonTaxiStore` і журнал; решта
  консолі працює з інтерфейсами.
- **Infrastructure → Application.** Сховище реалізує `ITaxiStore`, оголошений у прикладному
  шарі, — залежність спрямована до правил, а не від них.
- **Application → Domain.** Сервіси викликають правила домену й зберігають сутності.
- **Domain → нікуди.** У `TaxiDispatch.Domain.csproj` немає жодного `ProjectReference`: якщо
  домен спробує прочитати файл через інфраструктуру, проєкт не скомпілюється.

Чому жодна стрілка не веде всередину домену з інфраструктури: якби домен залежав від
інфраструктури, зміна формату файлу (JSON → SQLite) вимагала б перекомпіляції й перевірки
правил тарифу, хоча правила не змінювалися.

## 3. Композиційний корінь

Єдине місце, де створюються конкретні реалізації, — `src/TaxiDispatch.App/Composition.cs`:

```csharp
public static MainMenu CreateMenu(ConsoleIo io, ILoggerFactory logs, TimeProvider clock)
{
    var paths = new DataPaths("data", "state");
    var store = new JsonTaxiStore(paths);
    IFarePolicy fares = new TariffFarePolicy(JsonTariffSource.Load(paths));
    var orders = new OrderScreens(
        io,
        store.Places,
        new OrderIntakeService(store, fares, clock, logs.CreateLogger<OrderIntakeService>()),
        new DispatchService(store, fares, clock, logs.CreateLogger<DispatchService>()));
    var reports = new ReportScreens(
        io,
        store.Places,
        new OrderQueryService(store, logs.CreateLogger<OrderQueryService>()),
        new ShiftReportService(store, logs.CreateLogger<ShiftReportService>()));
    return new MainMenu(io, orders, reports);
}
```

Створення зібрано в одному місці, бо тоді решта коду знає лише інтерфейси: тести
підставляють `FakeStore` і `FixedTimeProvider` замість файлів і системного годинника, а заміна
сховища торкнеться одного рядка. Журнал і культура `uk-UA` налаштовуються в `Program.cs`.

## 4. Точки розширення

| Передбачувана зміна | Механізм | Що змінюється |
|---|---|---|
| Новий спосіб розрахунку ціни (коефіцієнт попиту, знижка постійним пасажирам) | інтерфейс `IFarePolicy` (стратегія) | новий клас у `Application/Pricing` і один рядок у `Composition`; сервіси не змінюються |
| Нові ціни або мінімальна вартість | дані `data/tariffs.json`, `TariffBook` перевіряє повноту | лише файл даних, код не змінюється |
| Інше сховище (SQLite, вебсервіс) | інтерфейс `ITaxiStore` з репозиторіями | новий клас в `Infrastructure` і рядок у `Composition` |
| Новий дозволений перехід стану | таблиця рішень `OrderTransitions._allowed` | один рядок таблиці й тест-рядок `[InlineData]` |
