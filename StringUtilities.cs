using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FCS.Utilities;

/// <summary>
/// String Utilities
/// </summary>
public static class StringUtilities
{
    /// <summary>
    /// Attempt To Convert String To An Integer
    /// Invalid Strings Return -1.
    /// </summary>
    /// <param name="stringIn"></param>
    public static int StringToInteger(this string stringIn)
    {
        bool Success = int.TryParse(stringIn, NumberStyles.Number, null, out int Result);   // Attempt To Convert String To An Integer

        return Success ? Result : -1;                                                       // Return Result Or -1 If Invalid
    }

    /// <summary>
    /// Convert a string to boolean - if possible
    /// </summary>
    /// <param name="trueOrFalse">String representing True or False</param>
    /// <returns>bool True Or False</returns>
    public static bool StringToBoolean(this string trueOrFalse)
    {
        if(trueOrFalse == null) return false;                                              // Return False If Null

        return (trueOrFalse.ToLower().Trim()) switch
        {
            "true" or "t" or "yes" or "y" or "1" or "-1" => true,                           // Return True For Common True Values
            _ => false,                                                                     // Return False As Default
        };
    }

    /// <summary>
    /// Convert a boolean value to "true" or "false" string
    /// </summary>
    /// <param name="trueOrFalse">Boolean Value</param>
    /// <returns>"true" or "false"</returns>
    public static string BooleanToString(this bool trueOrFalse)
    {
        if(trueOrFalse) return "true";                                                     // Return True

        return "false";                                                                     // Return False As Default
    }

    /// <summary>
    /// Sanitize string so that only characters 0-9, a-z and A-Z are returned.
    /// </summary>
    /// <param name="input">String To Sanitize</param>
    /// <returns>Sanitized String</returns>
    public static string CharsOnly(string? input = null)
    {
        if(input == null) return "";                                                       // Return Empty String If Null

        // Use Regular Expression To Pick Up Valid Characters
        Match Matches = Regex.Match(input, "[_0-9A-Za-z]*", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        string Chars = string.Empty;                                                        // Loop Through Matches And Build Return String

        while(Matches.Success)                                                             // Loop Through Matches And Build Return String
        {
            Chars += Matches.Value;                                                         // Add Valid Characters To Return String
            Matches = Matches.NextMatch();                                                  // Move To Next Match
        }

        return Chars;                                                                       // Return Valid Characters
    }

    /// <summary>
    /// Sanitize string so that only letters a-z and A-Z are returned.
    /// </summary>
    /// <param name="input">String To Sanitize</param>
    /// <returns>Sanitized String</returns>
    public static string LettersOnly(string? input = null)
    {
        if(input == null) return "";                                                       // Return Empty String If Null

        // Use Regular Expression To Pick Up Valid Characters
        Match Matches = Regex.Match(input, "[A-Za-z ]*", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        string Chars = string.Empty;                                                        // Loop Through Matches And Build Return String

        while(Matches.Success)
        {
            Chars += Matches.Value;                                                         // Add Valid Characters To Return String
            Matches = Matches.NextMatch();                                                  // Move To Next Match
        }

        return Chars;                                                                       // Return Valid Characters
    }

    ///// <summary>
    ///// Sanitize string so that only numbers 0-9 are returned.
    ///// </summary>
    ///// <param name="input">String To Sanitize</param>
    ///// <returns>Sanitized String</returns>
    //public static string NumbersOnly(string input = null)
    //{
    //    if(input == null) return "";

    //    Match Matches = Regex.Match(input, "[0-9]*", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
    //    string Chars = string.empty;

    //    while(Matches.Success)
    //    {
    //        Chars += Matches.Value;
    //        Matches = Matches.NextMatch();
    //    }

    //    return Chars;
    //}

    /// <summary>
    /// Sanitize string so that only numbers 0-9 are returned.
    /// </summary>
    /// <param name="input">String To Sanitize</param>
    /// <returns>Sanitized String</returns>
    public static string? NumbersOnly(string? input = null) => input != null ? new string([.. input.Where(char.IsDigit)]) : input;

    /// <summary>
    /// Sanitize string so that only numbers 0-9, period(.) and comma(,) are returned.
    /// </summary>
    /// <param name="input">String To Sanitize</param>
    /// <returns>Sanitized String</returns>
    public static string? DecimalOnly(string? input = null)
    {
        if(input == null) return "";                                                       // Return Empty String If Null

        // Use Regular Expression To Pick Up Valid Characters
        Match Matches = Regex.Match(input, @"[0-9\.\,]*", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        string Chars = string.Empty;                                                        // Loop Through Matches And Build Return String

        while(Matches.Success)                                                             // Loop Through Matches And Build Return String
        {
            Chars += Matches.Value;                                                         // Add Valid Characters To Return String
            Matches = Matches.NextMatch();                                                  // Move To Next Match
        }

        return Chars;                                                                       // Return Valid Characters
    }

    /// <summary>
    /// Sanitize string so that only numbers 0-9, period(.) and comma(,) are returned.
    /// </summary>
    /// <param name="input">String To Sanitize</param>
    /// <returns>Decimal Value</returns>
    public static decimal DecimalOnlyAsDecimal(string? input = null)
    {
        if(input == null) return 0;                                                        // Return 0 If Null

        // Use Regular Expression To Pick Up Valid Characters
        Match Matches = Regex.Match(input, @"[0-9\.\,]*", RegexOptions.IgnoreCase | RegexOptions.Multiline, TimeSpan.FromSeconds(1));
        string Chars = string.Empty;                                                        // Loop Through Matches And Build Return String

        while(Matches.Success)
        {
            Chars += Matches.Value;                                                         // Add Valid Characters To Return String
            Matches = Matches.NextMatch();                                                  // Move To Next Match
        }

        return TypeCasting.ToDecimal(Chars);                                                // Convert Valid Characters To Decimal
    }

    /// <summary>
    /// Return the current time as a short string
    /// </summary>
    /// <returns>Short Time String</returns>
    public static string ReturnShortStringTime()
    {
        DateTime tTime = DateTime.Now;                                                      // Get Current Time
        string tMonth = tTime.Month.ToString(CultureInfo.CurrentCulture);                   // Convert Month To String
        string tDay = tTime.Day.ToString(CultureInfo.CurrentCulture);                       // Convert Day To String
        string tHour = tTime.Hour.ToString(CultureInfo.CurrentCulture);                     // Convert Hour To String
        string tMinute = tTime.Minute.ToString(CultureInfo.CurrentCulture);                 // Convert Minute To String
        string tSecond = tTime.Second.ToString(CultureInfo.CurrentCulture);                 // Convert Second To String

        // Build Return String
        string tStr = tMonth;                                                               // Initialize With Month

        tStr += tDay.Length == 1 ? $"0{tDay}" : tDay;                                       // Add Day
        tStr += tHour.Length == 1 ? $"0{tHour}" : tHour;                                    // Add Hour
        tStr += tMinute.Length == 1 ? $"0{tMinute}" : tMinute;                              // Add Minutes
        tStr += tSecond.Length == 1 ? $"0{tSecond}" : tSecond;                              // Add Seconds

        return tStr;                                                                        // Return Short String
    }

    /// <summary>
    /// Retrieve QueryString Variables
    /// </summary>
    /// <param name="queryString"></param>
    /// <returns></returns>
    public static IDictionary<string, string> GetQueryStringParameters(string queryString)
    {
        if(queryString.StartsWith('?')) queryString = queryString[1..];              // Lets chop ofF the first character

        // Results Dictionary
        Dictionary<string, string> nameValueTable = new(StringComparer.OrdinalIgnoreCase);
        string[] nameValuePairs = queryString.Split('&');                         // Create Value Pairs Array

        foreach(string pair in nameValuePairs)                                             // Process Each Pair
        {
            string[] vars = pair.Split('=');                                      // Split Pair Into Strings

            if(!nameValueTable.ContainsKey(vars[0])) nameValueTable.Add(vars[0], vars[1]); // Add To Dictionary
        }

        return nameValueTable;                                                              // Return Dictionary
    }

    /// <summary>
    /// Return the starting numbers from the input string.
    /// Input: "123 Departures:
    /// Output: "123"
    /// </summary>
    /// <param name="txt">Input String To Be Evaluated</param>
    public static string ReturnInitialNumbers(string txt)
    {
        // Use Regular Expression To Pick Up Valid Characters
        Match tvMatch = Regex.Match(txt, @"\d+\.{0,1}\d*", RegexOptions.None, TimeSpan.FromSeconds(1));

        if(tvMatch.Success) return tvMatch.ToString();                                     // Return Valid Characters

        return "";                                                                          // Return Empty String If No Match
    }

    /// <summary>
    /// Return a string formatted for use in an SQL IN statement. E.g. "abc,def,ghi" becomes "'abc','def','ghi'"
    /// </summary>
    /// <param name="StringIn"></param>
    /// <returns></returns>
    public static string ReturnValidSQLInString(string StringIn)
    {
        // Check String Is Not Empty And Does Not Already Contain Apostrophes
        if((StringIn.Length > 0)
            && (!string.Equals(StringIn.Substring(1, 1), "'", StringComparison.OrdinalIgnoreCase)))
            return $"'{StringIn.Replace(",", "','", StringComparison.OrdinalIgnoreCase)}'"; // Delimit the guids with apostrophes

        return "'DUMMY_GUID'";  // Return A Dummy Value If String Is Empty Or Already Contains Apostrophes
    }

    #region MD5 Hash

    /// <summary>
    /// Calculate The MD5 Hash Of A String And Return As A Hex String
    /// </summary>
    /// <param name="inputBytes">Input byte array</param>
    /// <returns>MD5 hash as a hex string</returns>
    public static string CalculateMD5Hash(byte[] inputBytes)
    {
        byte[] hash = MD5.HashData(inputBytes);                                 // Convert Input String To Byte Array
        StringBuilder sb = new();                                               // Convert byte array to hex string

        for(int i = 0; i < hash.Length; i++)                                   // Process Each Character
            sb.AppendFormat(CultureInfo.CurrentCulture, "{0:X2}", hash[i]);     // Add Character To String

        return sb.ToString();                                                   // Return Hash
    }

    /// <summary>
    /// Calculates the MD5 hash of a string.
    /// </summary>
    /// <param name="inputString">The string to calculate the hash for.</param>
    /// <returns>The MD5 hash as a hexadecimal string.</returns>
    public static string CalculateMD5Hash(string inputString) => CalculateMD5Hash(Encoding.ASCII.GetBytes(inputString));

    /// <summary>
    /// Computes the MD5 hash of the input string and converts the first 8 bytes to a double-precision floating-point
    /// number.
    /// </summary>
    /// <remarks>MD5 is not cryptographically secure and should not be used for security purposes. Only the
    /// first 8 bytes of the 16-byte MD5 hash are used for the conversion.</remarks>
    /// <param name="inputString">The string to hash.</param>
    /// <returns>A double-precision floating-point number derived from the first 8 bytes of the MD5 hash.</returns>
    public static double CalculateMD5Hash_ToDouble(string inputString)
    {
        byte[] inputBytes = Encoding.ASCII.GetBytes(inputString);               // Convert Input String To Byte Array
        byte[] hash = MD5.HashData(inputBytes);                                 // Compute MD5 Hash

        return BitConverter.ToDouble(hash, 0);                                  // Convert First 8 Bytes To Double
    }

    /// <summary>
    /// Calculates the MD5 hash of the input bytes and returns it as a GUID.
    /// </summary>
    /// <param name="inputBytes">The byte array to hash.</param>
    /// <returns>A GUID containing the MD5 hash of the input bytes.</returns>
    public static Guid CalculateMD5Hash_ToGuid(byte[] inputBytes)
    {
        return new Guid(MD5.HashData(inputBytes));                               // Convert MD5 Hash To GUID
    }

    /// <summary>
    /// Computes the MD5 hash of a string and converts it to a GUID.
    /// </summary>
    /// <remarks>The input string is encoded using ASCII before hashing.</remarks>
    /// <param name="inputString">The string to hash.</param>
    /// <returns>A GUID created from the MD5 hash bytes.</returns>
    public static Guid CalculateMD5Hash_ToGuid(string inputString)
    {
        byte[] inputBytes = Encoding.ASCII.GetBytes(inputString);               // Convert Input String To Byte Array
        byte[] hash = MD5.HashData(inputBytes);                                 // Compute MD5 Hash

        return new Guid(hash);                                                  // Convert MD5 Hash To GUID
    }

    #endregion MD5 Hash

    /// <summary>
    /// Remove All Characters Which Are Not Valid In A File Name
    /// </summary>
    /// <param name="filename">String To Sanitize</param>
    /// <returns>Sanitized String</returns>
    public static string StripInvalidFileNameCharacters(string filename)
    {
        string tvValidFileName = string.Empty;                                      // Regular Expression To Pick Up Valid Characters - note that we are not using the standard invalid filename characters as this is too restrictive for TV show names etc. We are just removing characters which are not valid in Windows file names, but allowing other special characters which are valid in file names and commonly used in TV show names etc.
        const string tvRegExFilename = @"[^\\^\/^\:^\*^\?^\""^\<^\>^\|^\(^\)]*";    // Regular Expression Pattern

        // Use Regular Expression To Pick Up Valid Characters
        Match tvFileNameParts = Regex.Match(filename, tvRegExFilename,
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace,
            TimeSpan.FromSeconds(1));

        while(tvFileNameParts.Success)
        {
            tvValidFileName += tvFileNameParts.Groups[0].Value;                     // Add Valid Characters To Return String
            tvFileNameParts = tvFileNameParts.NextMatch();                          // Move To Next Match
        }

        return tvValidFileName;                                                     // Return Valid Characters
    }

    /// <summary>
    /// Returns a sub section of a connection string. E.g. Data Source
    /// </summary>
    /// <param name="text">Full connection string</param>
    /// <param name="search">Connection string parameter e.g. "Data Source"</param>
    /// <returns></returns>
    public static string? ReturnConnectionStringSegment(string text, string search)
    {
        search = search.Replace(" ", @"\s*", StringComparison.OrdinalIgnoreCase);       // Regular Expression To Pick Up Valid Characters
        string tvRegEx = search + @"=(?<section>[^\;]*)";                               // Regular Expression Pattern

        // Use Regular Expression To Pick Up Valid Characters
        Match tvMatch = Regex.Match(text, tvRegEx, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromSeconds(1));

        if(tvMatch.Success) return tvMatch.Groups["section"].Value;                    // Return Valid Characters

        return null;                                                                    // Return Null If No Match
    }

    /// <summary>
    /// Reverse The Contents Of A String
    /// </summary>
    /// <param name="s">String To Sanitize</param>
    /// <returns>Sanitized String</returns>
    public static string ReverseString(string s)
    {
        char[] arr = s.ToCharArray();       // Convert String To Character Array
        Array.Reverse(arr);                 // Reverse Character Array

        return new string(arr);             //
    }

    /// <summary>
    /// Return The First X Number Of Words In A String
    /// </summary>
    /// <param name="text">String To Sanitize</param>
    /// <param name="number">Number Of Words To Return</param>
    /// <returns></returns>
    public static string FirstWords(string text, int number)
    {
        string newString = string.Empty;                // New String To Build Results In

        try
        {
            string[] parts = text.Split(' ');           // Split String Into Words

            for(int i = 0; i < number; i++)            // Process Each Word
            {
                if(i <= number) newString += parts[i] + " ";        // Add To String
            }
        }
        catch
        {
        }

        return newString.Trim();                        // Return Results
    }

    /// <summary>
    /// Create A String With A Repeating Sequence
    /// </summary>
    /// <param name="text">Text Sequence</param>
    /// <param name="number">Number Of Times To Repeat</param>
    /// <returns>String</returns>
    public static string RepeatString(string text, int number)
    {
        string Result = string.Empty;                   // New String To Build Results In

        for(int i = 0; i < number; i++)                // Process Each Word
            Result += text;                             // Add To String

        return Result;                                  // Return Results
    }

    /// <summary>
    /// Convert String To Byte Array
    /// </summary>
    /// <param name="str">String To Convert</param>
    /// <returns>Byte Array</returns>
    public static byte[] GetBytes(string str)
    {
        byte[] bytes = new byte[str.Length * sizeof(char)];                 // Create Byte Array To Hold Result
        Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);     // Copy String Characters To Byte Array

        return bytes;                                                       // Return Byte Array
    }

    /// <summary>
    /// Convert Byte Array To String
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string GetString(byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];               // Create Character Array To Hold Result
        Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);                 // Copy Byte Array To Character Array

        return new string(chars);                                           // Return String
    }

    /// <summary>
    /// Convert String To Camel Case
    /// </summary>
    /// <param name="str">String To Be Process</param>
    /// <returns>Camel Case String</returns>
    public static string ToCamelCase(this string str)
    {
        // Check String Is Not Null Or Empty And Longer Than One Character
        return !string.IsNullOrEmpty(str) && str.Length > 1 ?
            char.ToLowerInvariant(str[0]) + str[1..] :
            str;
    }

    #region Passwords

    /// <summary>
    /// Generates a random password containing at least one digit, one uppercase letter, and one lowercase letter.
    /// </summary>
    /// <param name="passwordLength">The length of the password to generate.</param>
    /// <param name="includeSymbols">Whether to include special characters in addition to alphanumeric characters.</param>
    /// <returns>A randomly generated password string.</returns>
    public static string CreatePassword(int passwordLength, bool includeSymbols = false)
    {
        string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789"; //!@$?_-
        char[] chars = new char[passwordLength];                        // Create Character Array To Hold Result
        Random rd = new();                                              // Create Random Number Generator

        // If Symbols Are To Be Included Then Add These To The Allowed Characters
        if(includeSymbols) allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789 !#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        for(int i = 0; i < passwordLength; i++)                        // Process Each Character To Be Generated
        {
            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];   // Add Random Character To Array
        }

        // Check That The Password Contains At Least One Digit, One Uppercase Letter And One Lowercase Letter
        if(chars.Any(char.IsDigit) && chars.Any(char.IsLetter) && chars.Any(char.IsLower) && chars.Any(char.IsUpper)) return new string(chars);

        return CreatePassword(passwordLength, includeSymbols);          // If Not Then Try Again
    }

    /// <summary>
    /// Encrypts the input password using Base64 encoding.
    /// </summary>
    /// <param name="txtPassword">The password to encrypt.</param>
    /// <returns>The encrypted password as a Base64 string.</returns>
    public static string EncryptPassword(string txtPassword)
    {
        byte[] passBytes = Encoding.Unicode.GetBytes(txtPassword);      // Convert Password String To Byte Array

        return Convert.ToBase64String(passBytes);                       // Convert Byte Array To Base64 String And Return
    }

    /// <summary>
    /// To Decrypt password
    /// </summary>
    /// <param name="encryptedPassword"></param>
    /// <returns>It returns plain password</returns>
    public static string DecryptPassword(string encryptedPassword)
    {
        byte[] passByteData = Convert.FromBase64String(encryptedPassword);  // Convert Base64 String To Byte Array

        return Encoding.Unicode.GetString(passByteData);                    // Convert Byte Array To String And Return
    }

    #endregion Passwords

    /// <summary>
    /// Splits a camel case string by inserting spaces at case boundaries.
    /// </summary>
    /// <param name="str">The string to split.</param>
    /// <returns>The string with spaces inserted at camel case boundaries, or an empty string if <paramref name="str"/> is null.</returns>
    public static string SplitCamelCase(this string str)
    {
        if(str == null) return "";                                         // Return Empty String If Null

        str = str.Replace("_", "", StringComparison.OrdinalIgnoreCase);     // Remove Underscores
        str = str.Replace(".pdf", "", StringComparison.OrdinalIgnoreCase);  // Remove File Extension

        // Use Regular Expressions To Insert Spaces At Case Boundaries
        return Regex.Replace(
            Regex.Replace(
                str,
                @"(\P{Ll})(\P{Ll}\p{Ll})",
                "$1 $2", RegexOptions.None | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1)
            ),
            @"(\p{Ll})(\P{Ll})",
            "$1 $2", RegexOptions.None | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1)
        );
    }

    /// <summary>
    /// Compares two strings for equality ignoring whitespace and casing.
    /// </summary>
    /// <param name="string1">The first string to compare.</param>
    /// <param name="string2">The second string to compare.</param>
    /// <returns><c>true</c> if the strings contain the same characters ignoring whitespace and casing; otherwise, <c>false</c>.</returns>
    public static bool CompareCharactersOnly(string string1, string string2)
    {
        string1 ??= string.Empty;                                                                           // Ensure Null Strings Are Treated As Empty Strings
        string2 ??= string.Empty;                                                                           // Ensure Null Strings Are Treated As Empty Strings

        string _string1 = Regex.Replace(string1, @"\s", "", RegexOptions.None, TimeSpan.FromSeconds(1));    // Remove All Whitespace
        string _string2 = Regex.Replace(string2, @"\s", "", RegexOptions.None, TimeSpan.FromSeconds(1));    // Remove All Whitespace

        return _string1.Equals(_string2, StringComparison.OrdinalIgnoreCase);                               // Compare Strings Ignoring Case
    }

    /// <summary>
    /// Replaces the first occurrence of a specified string with another string using the provided comparison rule.
    /// </summary>
    /// <param name="Source">The string to search.</param>
    /// <param name="Find">The string to be replaced.</param>
    /// <param name="Replace">The string to replace the first occurrence of <paramref name="Find"/>.</param>
    /// <param name="stringComparison">One of the enumeration values that specifies the rules for the search.</param>
    /// <returns>A string with the first occurrence of <paramref name="Find"/> replaced by <paramref name="Replace"/>.</returns>
    public static string ReplaceFirstOccurrence(string Source, string Find, string Replace, StringComparison stringComparison)
    {
        int Place = Source.IndexOf(Find, stringComparison);                 // Find The First Occurrence Of The String To Replace

        return Source.Remove(Place, Find.Length).Insert(Place, Replace);    // Replace The First Occurrence Of The String To Replace
    }

    /// <summary>
    /// Replaces the last occurrence of a specified string with another string using the specified comparison type.
    /// </summary>
    /// <param name="Source">The string to search.</param>
    /// <param name="Find">The string to find.</param>
    /// <param name="Replace">The replacement string.</param>
    /// <param name="stringComparison">The string comparison type to use.</param>
    /// <returns>A new string with the last occurrence replaced.</returns>
    public static string ReplaceLastOccurrence(string Source, string Find, string Replace, StringComparison stringComparison)
    {
        int Place = Source.LastIndexOf(Find, stringComparison);             // Find The Last Occurrence Of The String To Replace

        return Source.Remove(Place, Find.Length).Insert(Place, Replace);    // Replace The Last Occurrence Of The String To Replace
    }

    /// <summary>
    /// Returns a string containing the messages of the provided exception and all inner exceptions, along with a cleaned-up stack trace.
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="webRoot"></param>
    /// <returns></returns>
    public static string GetFullExceptionMessage(Exception ex, string webRoot)
    {
        string message = "1. " + ex.Message + Environment.NewLine;          // Start With The Main Exception Message
        Exception? _innerException = ex.InnerException;                     // Get The Inner Exception (If Any)
        int cnt = 1;                                                        // Loop Through Inner Exceptions And Add Their Messages To The Result String

        while(_innerException != null)                                     // Loop Through Inner Exceptions And Add Their Messages To The Result String
        {
            cnt++;                                                          // Increment Counter

            // Add Message To Result String
            message += cnt.ToString(CultureInfo.CurrentCulture) + ". " + _innerException.Message + Environment.NewLine;
            _innerException = _innerException.InnerException;               // Move To Next Inner Exception
        }

        string stackTrace = ex.StackTrace ?? string.Empty;                  // Get Stack Trace And Clean It Up By Removing The Web Root And Common Folder Names To Make It Easier To Read

        // If Web Root Is Provided Then Remove This From The Stack Trace To Make It Easier To Read
        if(webRoot != "") stackTrace = stackTrace.Replace(webRoot, "", StringComparison.OrdinalIgnoreCase);

        // Remove Common Folder Names To Make Stack Trace Easier To Read
        stackTrace = stackTrace.Replace(".Controllers", "", StringComparison.OrdinalIgnoreCase)
            .Replace(@"\Areas\", "", StringComparison.OrdinalIgnoreCase)
            .Replace(@"\Controllers", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Controller", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Model", "", StringComparison.OrdinalIgnoreCase)
            .Replace(".cs", "", StringComparison.OrdinalIgnoreCase);

        // Add Stack Trace To Result String
        message += Environment.NewLine + "STACK TRACE" + Environment.NewLine + stackTrace;

        return message;                                                     // Return Result String
    }

    /// <summary>
    /// Removes HTML tags from a string using a regular expression. Note that this method does not handle all edge cases
    /// and may not be suitable for complex HTML content.
    /// </summary>
    /// <param name="input">The string containing HTML to strip.</param>
    /// <returns>The string with HTML tags removed.</returns>
    public static string StripHTML(string input) => Regex.Replace(input, "<.*?>", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1));

    /// <summary>
    /// Removes HTML tags from a string while converting BR tags to newlines and decoding HTML entities.
    /// </summary>
    /// <param name="input">The string containing HTML to strip.</param>
    /// <returns>The string with HTML tags removed, BR tags converted to newlines, and HTML entities decoded.</returns>
    public static string StripHTMLReplaceBR(string input)
    {
        input = Regex.Replace(input, @"<br\s*/*>", Environment.NewLine, RegexOptions.None, TimeSpan.FromSeconds(1));    // Replace <br> Tags With Newlines
        input = Regex.Replace(input, "<.*?>", string.Empty, RegexOptions.None, TimeSpan.FromSeconds(1));                // Remove All Other HTML Tags
        input = DecodeHtml(input) ?? string.Empty;                                                                      // Decode HTML Entities

        return input;   // Return Result String
    }

    /// <summary>
    /// Html Summary. Use @Html.Raw() when displaying the result of this method
    /// </summary>
    /// <param name="input"></param>
    /// <param name="length"></param>
    public static string StripHTML2(string input, int length)
    {
        if(input == null) return "";                               // Return Empty String If Null

        // Use Regular Expressions To Remove HTML Tags And Replace BR Tags With Newlines
        Regex lineBreakRegex = new(@"<(br|BR)\s{0,1}\/{0,1}>", RegexOptions.Multiline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));

        // This regex is designed to remove all HTML tags while preserving the content. It matches any sequence of characters that starts with '<',
        // followed by any characters that are not '>' (the tag name and attributes), and ends with '>'. The use of RegexOptions.Multiline allows
        // it to handle multi-line tags, and RegexOptions.ExplicitCapture ensures that only the entire match is captured, not any subgroups.
        Regex stripFormattingRegex = new("<[^>]*(>|$)", RegexOptions.Multiline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));

        // This regex is designed to remove whitespace and line breaks between HTML tags. It matches a '>' character followed by any combination
        // of non-word characters, newlines, or carriage returns, and then a '<' character. The use of RegexOptions.Multiline allows it to handle
        // multi-line content, and RegexOptions.ExplicitCapture ensures that only the entire match is captured.
        Regex tagWhiteSpaceRegex = new(@"(>|$)(\W|\n|\r)+<", RegexOptions.Multiline | RegexOptions.ExplicitCapture, TimeSpan.FromSeconds(1));
        string text = input;

        text = Regex.Replace(text, "<style.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(1));
        text = System.Net.WebUtility.HtmlDecode(text);              // Decode html specific characters
        text = tagWhiteSpaceRegex.Replace(text, "><");              // Remove tag whitespace/line breaks
        text = lineBreakRegex.Replace(text, Environment.NewLine);   // Replace <br /> with line breaks
        text = stripFormattingRegex.Replace(text, string.Empty);    // Strip formatting

        input = DecodeHtml(text) ?? string.Empty;                   // Decode HTML Entities

        // If The Resulting String Is Longer Than The Specified Length Then Truncate It And Replace Newlines With <br /> Tags For Display In HTML,
        // Otherwise Just Replace Newlines With <br /> Tags For Display In HTML
        if(input.Length > length) return input[..length].Replace(Environment.NewLine, "<br />", StringComparison.OrdinalIgnoreCase);

        // Replace Newlines With <br /> Tags For Display In HTML
        return input.Replace(Environment.NewLine, "<br />", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Decodes common HTML entities in a string to their corresponding characters.
    /// </summary>
    /// <param name="input">The string containing HTML entities to decode.</param>
    /// <returns>The decoded string with HTML entities replaced by their corresponding characters, or <see langword="null"/> if
    /// <paramref name="input"/> is <see langword="null"/>.</returns>

    public static string? DecodeHtml(string input)
    {
        if(input == null) return null;                                             // Return Null If Input Is Null

        return input.Replace("&gt;", ">", StringComparison.OrdinalIgnoreCase)       // Replace &gt; With >
            .Replace("&lt;", "<", StringComparison.OrdinalIgnoreCase)               // Replace &lt; With <
            .Replace("&amp;", "&", StringComparison.OrdinalIgnoreCase)              // Replace &amp; With &
            .Replace("&#39;", "'", StringComparison.OrdinalIgnoreCase);             // Replace &#39; With '
    }

    /// <summary>
    /// Removes or replaces characters that are problematic in CSV format, including newlines and commas, with spaces.
    /// </summary>
    /// <param name="input">The string to sanitize for CSV format.</param>
    /// <returns>A string with newline characters and commas replaced by spaces.</returns>
    public static string StripCharsForCsv(string input)
    {
        return input.Replace("\r\n", " ", StringComparison.OrdinalIgnoreCase)       // Replace Windows-style newlines with space
            .Replace("\n", " ", StringComparison.OrdinalIgnoreCase)                 // Replace Unix-style newlines with space
            .Replace("\r", " ", StringComparison.OrdinalIgnoreCase)                 // Replace old Mac-style newlines with space
            .Replace(",", " ", StringComparison.OrdinalIgnoreCase);                 // Replace commas with space to prevent CSV column issues
    }
}