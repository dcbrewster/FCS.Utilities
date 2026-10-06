using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Convert Text To Base64
    /// </summary>
    /// <param name="data">Text To Be Encoded</param>
    public static string TextToBase64(this string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// Convert Base64 To Text
    /// </summary>
    /// <param name="text">Encoded Text</param>
    public static string Base64ToText(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        return Encoding.UTF8.GetString(Convert.FromBase64String(text));
    }

    /// <summary>
    /// Read image file and return as byte array
    /// </summary>
    /// <param name="path">Full File Path</param>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
    public static byte[] ReadImageToBytes(this string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!File.Exists(path)) throw new FileNotFoundException($"Image file not found: {path}", path);

        return File.ReadAllBytes(path);
    }

    /// <summary>
    /// Convert Base64 Byte Array To An Image
    /// </summary>
    /// <param name="base64">Byte array containing image data</param>
    /// <exception cref="ArgumentNullException">Thrown when base64 is null</exception>
    /// <exception cref="ArgumentException">Thrown when base64 array is empty or invalid</exception>
    public static Image Base64ToImage(this byte[] base64)
    {
        ArgumentNullException.ThrowIfNull(base64);

        if (base64.Length == 0) throw new ArgumentException("Byte array cannot be empty", nameof(base64));

        using MemoryStream ms = new(base64);
        return Image.FromStream(ms);
    }
}