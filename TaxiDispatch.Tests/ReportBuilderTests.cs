using TaxiDispatch.Core.Reporting;

namespace TaxiDispatch.Tests;

// ЛР8, самоогляд PR: виправлений ReportBuilder не мав жодного тесту.
public class ReportBuilderTests
{
    [Fact]
    public void Build_TwoRows_TitleInUpperCaseThenRows()
    {
        var builder = new ReportBuilder { Title = "Звіт за день" };

        string text = builder.Build(["Замовлення 1", "Замовлення 2"]);

        string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        string[] expected = ["ЗВІТ ЗА ДЕНЬ", "Замовлення 1", "Замовлення 2"];
        Assert.Equal(expected, lines);
    }

    [Fact]
    public void Build_NullRows_Throws()
    {
        var builder = new ReportBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}
