using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Generate A Random Number Within A Range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive)</param>
    /// <param name="max">Maximum value (exclusive)</param>
    /// <returns>Random integer between min and max</returns>
    public static int RandomNumber(int min, int max)
    {
        if (min > max) min = max;                   // Ensure min is not greater than max

        return Random.Shared.Next(min, max);        // Generate and return random number
    }

    /// <summary>
    /// Generate A Random String With A Given Size.
    /// </summary>
    /// <param name="size">Length of the string to generate</param>
    /// <param name="lowerCase">True for lowercase letters, false for uppercase</param>
    /// <returns>Random alphabetic string</returns>
    public static string RandomString(int size, bool lowerCase = false)
    {
        if (size < 1) size = 1;                 // Ensure size is at least 1

        StringBuilder builder = new(size);      // Initialize StringBuilder with specified capacity
        char offset = lowerCase ? 'a' : 'A';    // Determine ASCII offset for letters
        const int lettersOffset = 26;           // Number of letters in the alphabet

        for (int i = 0; i < size; i++)          // Generate random character and append to builder
        {
            char character = (char)Random.Shared.Next(offset, offset + lettersOffset);  // Generate random letter
            builder.Append(character);          // Append character to the builder
        }

        return builder.ToString();
    }

    /// <summary>
    /// Generates a random password: 4 lowercase letters + 4 digits + 2 uppercase letters, shuffled.
    /// </summary>
    /// <returns>10-character randomized password</returns>
    public static string RandomPassword()
    {
        string tempValue = new StringBuilder()
            .Append(RandomString(4, true))              // 4 lowercase letters
            .Append(RandomNumber(1000, 9999))           // 4 digits
            .Append(RandomString(2))                    // 2 uppercase letters
            .ToString();                                // Combine parts into a single string

        return RandomizeString(tempValue);              // Shuffle the combined string to create the final password
    }

    // Regular expression to identify characters that are invalid in file names for the random pattern generation.
    [GeneratedRegex(@"[<>:""/\\|?*+=&]", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex InvalidFileNameCharsForRandomPattern();

    /// <summary>
    /// Generate a string suitable for use as a file name.
    /// </summary>
    /// <param name="size">Desired length of the file name</param>
    /// <returns>Random file-name-safe string</returns>
    public static string RandomFileName(int size)
    {
        if (size < 1) size = 1;                                     // Ensure size is at least 1

        byte[] bytes = RandomNumberGenerator.GetBytes(size + 50);   // Generate random bytes (size + 50 to ensure we have enough characters after filtering)
        string base64 = Convert.ToBase64String(bytes);              // Convert bytes to a Base64 string, which includes a mix of characters
        string result = InvalidFileNameCharsForRandomPattern().Replace(base64, string.Empty);   // Remove characters that are invalid in file names

        return result.Length >= size ? result[..size] : result;     // Return the first 'size' characters of the result, or the entire result if it's shorter than 'size'
    }

    /// <summary>
    /// Randomize (shuffle) the content of a string.
    /// </summary>
    /// <param name="inputString">String to be shuffled</param>
    /// <returns>Shuffled version of the input string</returns>
    public static string RandomizeString(string? inputString)
    {
        if (string.IsNullOrEmpty(inputString)) return string.Empty; // Return empty string if input is null or empty

        char[] chars = inputString.ToCharArray();                   // Convert string to character array for shuffling
        int n = chars.Length;                                       // Get the length of the character array

        // Fisher-Yates shuffle algorithm
        for (int i = n - 1; i > 0; i--)                             // Iterate from the end of the array to the beginning
        {
            int j = Random.Shared.Next(i + 1);                      // Generate a random index from 0 to i

            (chars[i], chars[j]) = (chars[j], chars[i]);            // Swap the characters at indices i and j
        }

        return new string(chars);                                   // Convert the shuffled character array back to a string and return it
    }
}