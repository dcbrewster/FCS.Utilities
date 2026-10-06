using System;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Return a substring marked with START and END markers
    /// </summary>
    /// <param name="sectionName">Name of the section to extract</param>
    /// <param name="template">Template string containing the section</param>
    /// <returns>Extracted section or empty string if not found</returns>
    public static string ExtractSection(string sectionName, string template)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(template);

        string startName = $"<!-- {sectionName.ToUpper()} START -->";
        string endName = $"<!-- {sectionName.ToUpper()} END -->";
        int startPos = template.IndexOf(startName, StringComparison.OrdinalIgnoreCase);
        int endPos = template.IndexOf(endName, StringComparison.OrdinalIgnoreCase);

        if (startPos < 0 || endPos < 0 || startPos >= endPos) return string.Empty;

        return template.Substring(startPos, endPos - startPos + endName.Length);
    }
}