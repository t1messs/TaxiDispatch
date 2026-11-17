using TaxiDispatch.Application.Validation;

namespace TaxiDispatch.App;

/// <summary>
/// Ввід і вивід застосунку поверх <see cref="TextReader"/> і <see cref="TextWriter"/>:
/// у тестах їх можна підмінити рядками. Помилка вводу — повідомлення й повторний запит.
/// </summary>
internal sealed class ConsoleIo
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleIo(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
    }

    public void Say(string text) => _output.WriteLine(text);

    /// <summary>Питає рядок; кінець вводу (Ctrl+D) завершує роботу застосунку.</summary>
    public string Read(string prompt)
    {
        _output.Write($"{prompt}: ");
        return _input.ReadLine() ?? throw new EndOfStreamException("Ввід завершено.");
    }

    /// <summary>Питає, доки барикада не прийме відповідь.</summary>
    /// <typeparam name="T">Тип значення, яке повертає барикада.</typeparam>
    public T Ask<T>(string prompt, Func<string?, Result<T>> parse)
    {
        while (true)
        {
            Result<T> answer = parse(Read(prompt));
            if (answer.IsSuccess)
            {
                return answer.Value!;
            }

            Say($"  ! {answer.Error}");
        }
    }
}
