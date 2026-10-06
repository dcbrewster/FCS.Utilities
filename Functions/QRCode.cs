using System;
using System.Drawing;
using System.IO;
using System.Text;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;
using static FCS.Utilities.Function;

namespace FCS.Utilities;

/// <summary>
/// Create and Read QR Codes
/// </summary>
public class QRCode
{
    /// <summary>
    /// Default Constructor
    /// </summary>
    public QRCode()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="imageFileName">Full File Path To Image File</param>
    /// <param name="content">Content of QR Code</param>
    public QRCode(string imageFileName, string content) => Create(imageFileName, content);

    /// <summary>
    /// Create A QR Code
    /// </summary>
    /// <param name="imageFileName">File name for the image (will be saved under WebPath)</param>
    /// <param name="content">Content of QR Code</param>
    /// <param name="width">Width of QRCode (minimum 100)</param>
    /// <param name="height">Height of QRCode (minimum 100)</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
    /// <exception cref="IOException">Thrown when file cannot be saved</exception>
    public static void Create(string imageFileName, string content, int width = 500, int height = 500)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);

        if (width < 100) throw new ArgumentException("Width must be at least 100 pixels", nameof(width));

        if (height < 100) throw new ArgumentException("Height must be at least 100 pixels", nameof(height));

        QrCodeEncodingOptions options = new()
        {
            DisableECI = true,
            CharacterSet = "UTF-8",
            Width = width,
            Height = height
        };

        BarcodeWriter writer = new()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = options
        };

        using Bitmap qrCodeBitmap = writer.Write(content);
        string fullPath = Path.Combine(WebPath, imageFileName);

        try
        {
            qrCodeBitmap.Save(fullPath);
        }
        catch (Exception ex)
        {
            throw new IOException($"Unable to save QR code image to '{fullPath}'", ex);
        }
    }

    /// <summary>
    /// Return Contents Of QR Code
    /// </summary>
    /// <param name="imageFileName">Full path to the QR code image file</param>
    /// <returns>Decoded content string, or empty string if file not found</returns>
    /// <exception cref="IOException">Thrown when file cannot be read or decoded</exception>
    public static string Read(string? imageFileName = null)
    {
        if (string.IsNullOrWhiteSpace(imageFileName) || !File.Exists(imageFileName)) return string.Empty;

        DecodingOptions options = new()
        {
            PossibleFormats = [BarcodeFormat.QR_CODE],
            TryHarder = true
        };

        try
        {
            using Bitmap qrCodeBitmap = new(imageFileName);
            BarcodeReader reader = new()
            {
                Options = options
            };

            Result? result = reader.Decode(qrCodeBitmap);

            return result?.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new IOException($"Unable to read QR code from '{imageFileName}'", ex);
        }
    }

    /// <summary>
    /// Add Contact Information To QRCode
    /// </summary>
    /// <param name="imageFileName">File name for the contact QR code</param>
    /// <param name="name">Contact name</param>
    /// <param name="telephone">Contact phone number</param>
    /// <param name="email">Contact email address</param>
    /// <param name="address">Contact address</param>
    /// <param name="notes">Additional notes</param>
    public static void CreateContact(string imageFileName, string name, string telephone, string email, string address, string notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageFileName);
        StringBuilder contactInfo = new();

        contactInfo.Append("MECARD:");

        if (!string.IsNullOrEmpty(name)) contactInfo.Append($"N:{name};");
        if (!string.IsNullOrEmpty(telephone)) contactInfo.Append($"TEL:{telephone};");
        if (!string.IsNullOrEmpty(email)) contactInfo.Append($"EMAIL:{email};");
        if (!string.IsNullOrEmpty(address)) contactInfo.Append($"ADR:{address};");
        if (!string.IsNullOrEmpty(notes)) contactInfo.Append($"NOTE:{notes}");

        Create(imageFileName, contactInfo.ToString());
    }
}