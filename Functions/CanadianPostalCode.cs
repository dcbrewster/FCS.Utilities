using System;
using System.Text.RegularExpressions;

namespace FCS.Utilities;

public static partial class Function
{
    [GeneratedRegex(@"[ABCEGHJKLMNPRSTVXY][0-9][ABCEGHJKLMNPRSTVWXYZ]\s[0-9][ABCEGHJKLMNPRSTVWXYZ][0-9]", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CanadianPostalCodePattern();

    /// <summary>
    /// Validate the format of the postal code to ensure it meets Canada Post requirements.
    /// </summary>
    /// <param name="postalCode">Postal Code To Be Inspected</param>
    /// <returns>Validation result with formatted postal code</returns>
    public static PostalCodeValidationResult ValidateCanadianPostalCode(this string postalCode)
    {
        postalCode = postalCode.FormatCanadianPostalCode();             // Make sure postal code is in proper format

        bool isValid = CanadianPostalCodePattern().IsMatch(postalCode); // Validate the postal code against the pattern

        return new PostalCodeValidationResult(isValid, postalCode);     // Return the validation result along with the formatted postal code
    }

    /// <summary>
    /// Format a string into a Canadian Postal Code format
    /// Format: "A9A 9A9"
    /// </summary>
    /// <param name="postalCode">Postal Code To Be Inspected</param>
    public static string FormatCanadianPostalCode(this string postalCode)
    {
        if (postalCode is null) return string.Empty;

        // Make sure postal code is all upper case and remove any spaces.
        postalCode = postalCode.Trim().ToUpper().Replace(" ", "", StringComparison.Ordinal);

        // Pad the postal code to 6 characters if required
        if (postalCode.Length < 6) postalCode = postalCode.PadRight(6, '#');

        // Reformat Postal Code To "XXX XXX"
        postalCode = $"{postalCode.AsSpan()[..3]} {postalCode.AsSpan(3, 3)}";

        if (string.Equals(postalCode, "### ###", StringComparison.Ordinal)) postalCode = string.Empty;

        return postalCode;
    }

    /// <summary>
    /// Format Postal Code To All Caps But No Space
    /// </summary>
    /// <param name="postalCode">Postal Code To Be Inspected</param>
    /// <returns>Postal Code in A9A9A9 Format</returns>
    public static string FormatCanadianPostalCodeShort(this string postalCode)
    {
        string formattedPostalCode = FormatCanadianPostalCode(postalCode);

        return formattedPostalCode.Replace(" ", "", StringComparison.Ordinal);
    }

    /// <summary>
    /// Format The Postal Code For Canada, Then Check To See If It May Actually Be An
    /// American Post Code. If The Last Character of the Postal Code is "#",
    /// Remove The "#" And Return The Remainder
    /// </summary>
    /// <param name="postalCode">Postal Code Value</param>
    public static string FormatCanadianOrUsPostalCode(this string postalCode)
    {
        string result = FormatCanadianPostalCode(postalCode);

        return result.Replace("#", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal);
    }
}

/// <summary>
/// Result of postal code validation
/// </summary>
/// <param name="IsValid">Whether the postal code is valid</param>
/// <param name="FormattedPostalCode">The formatted postal code</param>
public readonly record struct PostalCodeValidationResult(bool IsValid, string FormattedPostalCode);