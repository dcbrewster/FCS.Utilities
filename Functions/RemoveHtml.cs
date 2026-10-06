using System.Text.RegularExpressions;

namespace FCS.Utilities;

public static partial class Function
{
    [GeneratedRegex(@"<[^>]*>", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HtmlTagsPattern();

    [GeneratedRegex(@"&nbsp;", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NbspPattern();

    [GeneratedRegex(@"\s{2,}", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex MultipleWhitespacePattern();

    /// <summary>
    /// Strip HTML tags and collapse whitespace.
    /// </summary>
    /// <param name="stringIn">HTML string to clean</param>
    /// <returns>Plain text with single spaces</returns>
    public static string? RemoveHtml(this string? stringIn)
    {
        if (string.IsNullOrWhiteSpace(stringIn)) return stringIn;

        string result = HtmlTagsPattern().Replace(stringIn, " ");   // Remove HTML tags, replace with space to avoid concatenating words.
        result = NbspPattern().Replace(result, " ");                // Replace &nbsp; with space.
        result = MultipleWhitespacePattern().Replace(result, " ");  // Collapse multiple whitespace into single space.

        return result.Trim();                                       // Trim leading and trailing whitespace.
    }
}