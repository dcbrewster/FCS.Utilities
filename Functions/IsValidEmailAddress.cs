using MimeKit;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Validates an email address using RFC 5322 parsing via MailKit.
    /// This provides robust validation that handles international addresses,
    /// quoted strings, and other edge cases better than regex-based approaches.
    /// </summary>
    /// <param name="emailAddress">Email address to validate</param>
    /// <returns>true if valid email format per RFC 5322, false otherwise</returns>
    public static bool IsValidEmailAddress(this string? emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress)) return false;

        // Use MailKit's MailboxAddress parser which implements RFC 5322
        // Verify that the parsed address matches the input to reject display names
        return MailboxAddress.TryParse(emailAddress, out MailboxAddress? mailbox)
               && mailbox.Address == emailAddress;
    }
}