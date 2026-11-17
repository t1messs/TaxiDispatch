using System.Reflection;

namespace TaxiDispatch.Core;

/// <summary>
/// Версія збірки з <c>Directory.Build.props</c>.
/// </summary>
public static class VersionInfo
{
    /// <summary>Повертає версію без суфікса коміту, наприклад 0.8.0.</summary>
    /// <returns>Рядок версії.</returns>
    public static string Current()
    {
        var assembly = typeof(VersionInfo).Assembly;
        var attribute = assembly.GetCustomAttribute<
            AssemblyInformationalVersionAttribute>();
        var raw = attribute?.InformationalVersion ?? "0.0.0";
        var plus = raw.IndexOf('+');
        return plus < 0 ? raw : raw[..plus];
    }
}
