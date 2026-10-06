using System.Globalization;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Converts the specified string to title case using the specified culture.
    /// </summary>
    /// <param name="stringIn">The string to convert to title case.</param>
    /// <param name="cultureName">The name of the culture to use for the conversion. Defaults to "en-CA".</param>
    /// <returns>The string converted to title case, or <see cref="string.Empty"/> if the input is <see langword="null"/> or
    /// whitespace.</returns>
    public static string ProperCase(this string? stringIn, string cultureName = "en-CA")
    {
        if (string.IsNullOrWhiteSpace(stringIn)) return string.Empty;

        return new CultureInfo(cultureName, false).TextInfo.ToTitleCase(stringIn.Trim());
    }
}