using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FCS.Utilities;

public static partial class Function
{
    [GeneratedRegex("[<>:|?*|&]", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex InvalidFileNameCharsPattern();

    /// <summary>
    /// <para>Validate that the supplied File Name is valid so it can be used within a URL.</para>
    /// <para>Paths can be up to 32,000 characters.</para>
    /// <para>
    /// Forbids the use of characters in range 1-31 (0x01-0x1F) and characters " * / : Less Than Symbol, Greater Than Symbol ? \ | "
    /// unless the name is flagged as being in the Posix namespace. NTFS allows each path component
    /// (directory or filename) to be 255 characters long.
    /// </para>
    /// <para>
    /// Windows forbids the use of the MS-DOS device names AUX, CLOCK$, COM1, …, COM9, CON, LPT1, …,
    /// LPT9, NUL and PRN, as well as these names with any extension(for example, AUX.txt), except
    /// when using Long UNC paths(ex. \\.\C:\nul.txt or \\?\D:\aux\con). (CLOCK$ may be used, if an
    /// extension is provided.) The Win32 API strips trailing space and period(full-stop) characters
    /// from filenames, except when UNC paths are used.These restrictions only apply to Windows; in
    /// Linux distributions that support NTFS, filenames are written using NTFS's Posix namespace,
    /// which allows any Unicode character except / and NUL.
    /// </para>
    /// <para>Also added ampersand as a restrictive character - URL problems.</para>
    /// </summary>
    /// <param name="fileName">Filename to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValidFileName(this string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;      // Null, empty, or whitespace-only strings are not valid file names

        fileName = Path.GetFileName(fileName);                      // Extract just the file name component to check against reserved names and invalid characters

        if (string.IsNullOrEmpty(fileName)) return false;           // If the file name is empty after extraction, it's not valid

        // Check for reserved file names (case-insensitive)
        if (ReservedFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase)) return false;

        return !InvalidFileNameCharsPattern().IsMatch(fileName);    // Check for invalid characters using the regex pattern
    }

    // Reserved File Names
    private static readonly string[] ReservedFileNames = [
        "AUX",
        "CLOCK$",
        "CON",
        "PRN",
        "NUL",
        "COM1",
        "COM2",
        "COM3",
        "COM4",
        "COM5",
        "COM6",
        "COM7",
        "COM8",
        "COM9",
        "LPT1",
        "LPT2",
        "LPT3",
        "LPT4",
        "LPT5",
        "LPT6",
        "LPT7",
        "LPT8",
        "LPT9"
    ];
}