using System;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Convert text line breaks (CRLF, LF, CR) to HTML br tags
    /// </summary>
    /// <param name="stringIn">Input String</param>
    /// <returns>String with HTML br tags</returns>
    public static string FormatTextForHtml(this string? stringIn)
    {
        if(string.IsNullOrWhiteSpace(stringIn)) return string.Empty;

        // Handle all line break variations
        return stringIn
            .Replace("\r\n", "<br />", StringComparison.Ordinal)  // Windows CRLF
            .Replace("\n", "<br />", StringComparison.Ordinal)    // Unix LF
            .Replace("\r", "<br />", StringComparison.Ordinal);   // Mac CR
    }
}