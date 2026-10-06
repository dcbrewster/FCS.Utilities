using System;
using System.Globalization;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Format supplied address for Post Office use.
    /// </summary>
    /// <param name="line1">Line 1 Of Address</param>
    /// <param name="line2">Line 2 Of Address</param>
    /// <param name="city">City</param>
    /// <param name="province">Province or State</param>
    /// <param name="postal">Postal Code</param>
    /// <param name="country">Country</param>
    /// <param name="html">True: Return Address Within Html</param>
    /// <returns>Address Formatted For Post Office</returns>
    public static string PostalAddress(string? line1, string? line2, string? city, string? province, string? postal, string? country, bool html = false)
    {
        string result = string.IsNullOrEmpty(line1) ? string.Empty : $"{line1!.FormatName()!.Trim()}\n";
        result += string.IsNullOrEmpty(line2) ? string.Empty : $"{line2!.FormatName()!.Trim()}\n";
        result += string.IsNullOrEmpty(city) ? string.Empty : $"{city!.FormatName()!.Trim()}";
        result += string.IsNullOrEmpty(province) ? string.Empty : $", {province!.ToUpper(CultureInfo.CurrentCulture).Trim()}";
        result += string.IsNullOrEmpty(postal) ? string.Empty : $" {postal!.FormatCanadianOrUsPostalCode().Trim()}\n";
        result += string.IsNullOrEmpty(country) ? string.Empty : $"{country!.FormatName()!.Trim()}\n";

        if (!html) return result;

        return $"<div>\n\t{result.Replace("\n", "<br>\n", StringComparison.Ordinal)}</div>\n";
    }
}