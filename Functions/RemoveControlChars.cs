using System.Text.RegularExpressions;

namespace FCS.Utilities;

public static partial class Function
{
    [GeneratedRegex(@"[\p{C}-[\r\n]]+", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ControlCharsPattern();

    /// <summary>
    /// Extract all non-printable control characters from the supplied string.
    /// Exception: CR and LF are maintained.
    /// </summary>
    /// <param name="stringIn">Input string</param>
    /// <returns>String with control characters removed</returns>
    public static string? RemoveControlChars(this string? stringIn)
    {
        if (string.IsNullOrWhiteSpace(stringIn)) return stringIn;       // Return null or empty string as is.

        return ControlCharsPattern().Replace(stringIn, string.Empty);   // Remove control characters, except CR and LF.
    }
}