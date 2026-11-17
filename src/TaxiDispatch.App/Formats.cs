namespace TaxiDispatch.App;

/// <summary>
/// Однакове форматування чисел і часу у виводі (культура — uk-UA, задається в Program).
/// </summary>
internal static class Formats
{
    public static string Money(decimal amount) => $"{amount:N2} грн";

    public static string Km(decimal km) => $"{km:0.0##} км";

    public static string Time(DateTimeOffset moment) => $"{moment:dd.MM.yyyy HH:mm}";
}
