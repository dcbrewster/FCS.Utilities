using System;
using System.IO;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Validates if the supplied path has valid syntax.
    /// Does not check if the path exists on disk.
    /// Supports Windows (C:\path), UNC (\\server\share), Unix (/path), and relative paths.
    /// </summary>
    /// <param name="path">Path to validate</param>
    /// <returns>True if the path syntax is valid, false otherwise</returns>
    public static bool IsValidPath(this string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        try
        {
            // Path.GetFullPath will throw on invalid paths
            // This validates syntax without checking disk existence
            _ = Path.GetFullPath(path);
            return true;
        }
        catch (ArgumentException)
        {
            // Invalid characters or format
            // Note: On Windows, this also catches paths with invalid drive letters (e.g., "Z:\") since they are considered invalid syntax
            return false;
        }
        catch (NotSupportedException)
        {
            // Colon in middle of path (Windows) or other unsupported format
            // Note: On Windows, this also catches paths with invalid drive letters (e.g., "Z:\") since they are considered invalid syntax
            return false;
        }
        catch (PathTooLongException)
        {
            // Path exceeds system limit
            return false;
        }
    }

    /// <summary>
    /// Validates if the supplied path has valid syntax AND the root drive/directory exists.
    /// Checks both path format validity and whether the root (drive letter, UNC server, or Unix root) exists.
    /// Supports Windows (C:\path), UNC (\\server\share), Unix (/path), and relative paths.
    /// </summary>
    /// <param name="path">Path to validate</param>
    /// <returns>True if the path syntax is valid and the root exists, false otherwise</returns>
    public static bool IsValidPathWithExistingRoot(this string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;          // Empty or whitespace paths are invalid

        try
        {
            string fullPath = Path.GetFullPath(path);               // Validate syntax first; will throw if invalid
            string? root = Path.GetPathRoot(fullPath);              // Get the root (e.g., "C:\", "\\server\share\", "/") for existence check

            return root != null && Directory.Exists(root);          // Directory.Exists returns true for existing drives, UNC shares, and Unix root; false otherwise
        }
        catch (ArgumentException)
        {
            return false;                                           // Invalid characters or format
        }
        catch (NotSupportedException)
        {
            return false;                                           // Colon in middle of path (Windows) or other unsupported format
        }
        catch (PathTooLongException)
        {
            return false;                                           // Path exceeds system limit
        }
    }
}