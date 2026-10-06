using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FCS.Utilities;

public static partial class Function
{
    [GeneratedRegex(@"(?<=\b(?:mc|mac)?)[a-zA-Z]", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NameCapitalizationPattern();

    /// <summary>
    /// <para>
    /// Formats a name (or any string) so that the first character is capitalized and the
    /// remainder of the string is lower case.
    /// </para>
    /// <para>Handles names with "Mc", "Mac", "O'", "D'"</para>
    /// <para>Probably others "de", "du" that should be added.</para>
    /// <para>Use as method (string.FormatName()) or function (FormatName("String"))</para>
    /// </summary>
    /// <param name="name">The name to format</param>
    /// <returns>Formatted String</returns>
    public static string? FormatName(this string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        List<string> noTouch = ["FCS"];                                                                             // These Words Must Be Returned In Lower Case.
        List<string> allCaps = [];                                                                                  // Acronyms That Contain Vowels - Return All Upper Case
        List<string> noAllCaps = ["Jr", "Rd", "St"];                                                                // Words WithOut Vowels - But Must Be Formatted
        char[] punctuation = [.. name.Where(char.IsPunctuation).Distinct()];                                        // Get All Unique Punctuation Characters In The Name
        string[] words = [.. name.ToLower(CultureInfo.CurrentCulture).Split().Select(x => x.Trim(punctuation))];    // Split Name Into Words And Remove Punctuation
        StringBuilder result = new();                                                                               // String Builder For Result

        foreach (string word in words)
        {
            if (noTouch.Contains(word, StringComparer.Ordinal))
            {
                result.Append(' ').Append(word);
            }
            else
            {
                // First Character To Upper Case
                string term = NameCapitalizationPattern().Replace(word, m => m.Value.ToUpper(CultureInfo.CurrentCulture));

                term = term.Replace("Iii", "III", StringComparison.OrdinalIgnoreCase);
                term = term.Replace("Ii", "II", StringComparison.OrdinalIgnoreCase);
                term = term.Replace("Du ", "du ", StringComparison.OrdinalIgnoreCase);
                term = term.Replace("'S", "'s", StringComparison.OrdinalIgnoreCase);

                // If (No Vowels And Not In NoAllCaps List) Or in AllCaps List
                if ((term.Any("aeiouyAEIOUY".Contains)
                    && noAllCaps.Contains(term, StringComparer.Ordinal))
                    || allCaps.Contains(term.ToUpper(CultureInfo.CurrentCulture), StringComparer.Ordinal))
                    term = term.ToUpper(CultureInfo.CurrentCulture);

                result.Append(' ').Append(term);
            }
        }

        return result.ToString().Trim();
    }

#if DEBUG

    /// <summary>
    /// Formats the name test.
    /// </summary>
    /// <returns>String with results</returns>
    public static string FormatNameTest()
    {
        string[] testCases =
        [
            "dave's",
            "mcfly",
            "mc fly",
            "macfly",
            "mac fly",
            "o'dell",
            "O'Riely",
            "mac jones ii",
            "Mac jones jr.",
            "Mac jones sr.",
            "dr. Mac jones jr.",
            "Mac jones ii"
        ];

        StringBuilder builder = new();

        foreach (string testCase in testCases)
        {
            builder.Append('\'')
                .Append(testCase)
                .Append("' is now '")
                .Append(FormatName(testCase))
                .Append('\n');
        }

        return builder.ToString();
    }

#endif
}