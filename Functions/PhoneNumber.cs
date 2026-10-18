using System;
using System.Linq;

namespace FCS.Utilities;

public static partial class Function
{
    private const int NorthAmericanPhoneLength = 10;        // 10 digits for North American phone numbers (Area Code + Exchange + Line Number)
    private const int AreaCodeLength = 3;                   // 3 digits for Area Code
    private const int ExchangeLength = 3;                   // 3 digits for Exchange

    /// <summary>
    /// Formats the phone number into North American style.
    /// Use as method (string.FormatPhoneNumber()) or function (FormatPhoneNumber("String"))
    /// </summary>
    /// <param name="phone">The Phone Number string.</param>
    /// <param name="option">0 for "123-456-7890" or 1 for "(123) 456-7890"</param>
    /// <returns>Formatted Phone Number string</returns>
    public static string? FormatPhoneNumber(this string phone, int option = 0)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;                          // Return null for empty or whitespace input

        // Look For Extension Number
        int extIndex = phone.IndexOf("ext", StringComparison.OrdinalIgnoreCase);    // Check For "ext" (case-insensitive) To Separate Extension From Main Phone Number
        string extension = string.Empty;                                            // If Extension Found, Separate It From The Main Phone Number

        if (extIndex > -1)                                                          // If "ext" Found, Extract Extension And Main Phone Number
        {
            extension = phone[extIndex..];                                          // Extract Extension (everything after "ext")
            phone = phone[..extIndex];                                              // Extract Main Phone Number (everything before "ext")
        }

        string? strippedPhone = phone.StripPhoneNumber();                           // Strip Main Phone Number To Digits Only

        if (string.IsNullOrEmpty(strippedPhone)) return null;                       // Return null if no digits found in the main phone number

        phone = strippedPhone;                                                      // Use Stripped Phone Number For Formatting
        extension = extension.StripPhoneNumber() ?? string.Empty;                   // Strip Extension To Digits Only (if any)

        // Pad Phone Number With Leading Zeros If Less Than 10 Digits
        if (phone.Length < NorthAmericanPhoneLength) phone = phone.PadLeft(NorthAmericanPhoneLength, '0');

        try
        {
            if (option == 1)
            {
                // Format: (123) 456-7890
                phone = $"({phone[..AreaCodeLength]}) {phone.Substring(AreaCodeLength, ExchangeLength)}-{phone[(AreaCodeLength + ExchangeLength)..]}";

                // Remove 000 Area Code?
                if (string.Equals(phone[..5], "(000)", StringComparison.Ordinal)) phone = phone[5..];

                phone = phone.Trim();
            }
            else
            {
                // Format: 123-456-7890
                phone = $"{phone[..AreaCodeLength]}-{phone.Substring(AreaCodeLength, ExchangeLength)}-{phone[(AreaCodeLength + ExchangeLength)..]}";

                // Remove 000 Area Code?
                if (string.Equals(phone[..4], "000-", StringComparison.Ordinal)) phone = phone[4..];
            }
        }
        catch (Exception)
        {
            return phone;   // If Formatting Fails For Any Reason, Return The Stripped Phone Number As Is (Without Formatting)
        }

        // Add Extension Back To Phone Number
        if (!string.IsNullOrEmpty(extension)) phone += $" Ext. {extension}";

        return phone;       // Return Formatted Phone Number With Extension (If Any)
    }

    /// <summary>
    /// Strips the phone number to digits only.
    /// <para>Use as method (string.StripPhoneNumber()) or function (StripPhoneNumber("String"))</para>
    /// </summary>
    /// <param name="phone">The Phone Number string.</param>
    /// <returns>Digits only of a 10 digit phone number 1234567890</returns>
    public static string? StripPhoneNumber(this string? phone)
    {
        if (phone is null) return null;                     // Return null if input is null

        phone = new string([.. phone.Where(char.IsDigit)]); // Remove all non-digit characters from the phone number

        // If the resulting phone number has more than 10 digits, keep only the last 10 digits (to handle cases with country codes or extensions)
        if (phone.Length > NorthAmericanPhoneLength) phone = phone.Substring(phone.Length - NorthAmericanPhoneLength, NorthAmericanPhoneLength);

        return phone;                                       // Return the stripped phone number (digits only)
    }
}