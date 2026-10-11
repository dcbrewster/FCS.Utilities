using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using static FCS.Utilities.Function;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace FCS.Utilities;

/// <summary>
/// Send Email Message
/// </summary>
public class EmailHelper
{
    private static readonly Regex HtmlTagRegex = new("<[^>]+>", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

    #region Public Properties

    /// <summary>
    /// List of ToAddress
    /// </summary>
    public List<Tuple<string, string>> ToAddressList = [];

    /// <summary>
    /// List of CCAddress
    /// </summary>
    public List<Tuple<string, string>> CCAddressList = [];

    /// <summary>
    /// List of BCCAddress
    /// </summary>
    public List<Tuple<string, string>> BCCAddressList = [];

    /// <summary>
    /// List of ReplyAddress
    /// </summary>
    public List<Tuple<string, string>> ReplyToAddressList = [];

    /// <summary>
    /// List of file attachments
    /// </summary>
    public List<string> Attachments = [];

    /// <summary>
    /// List of inline attachments. Typically used to embed images into html
    /// </summary>
    public List<InlineAttachment> InlineAttachments = [];

    /// <summary>
    /// List of String Attachments
    /// </summary>
    public List<StringAttachment> StringAttachments = [];

    /// <summary>
    /// Constructor
    /// </summary>
    public EmailHelper()
    {
        Priority = MessagePriority.Normal;
        Importance = MessageImportance.Normal;
        Headers =
        [
            // Add our header to the Header List
            new Header {Name = "X-SmtpEmail", Value = $"Copyright Forest City Software 1999 - {DateTime.Now.Year}" }
        ];
    }

    /// <summary>
    /// Email's "From" Address
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Email's "From" Name
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Email Subject
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email Body Text
    /// </summary>
    public string TextBody { get; set; } = string.Empty;

    /// <summary>
    /// Html body content
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Smtp Server url or IP Address
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Smtp Port Typically port 25 - unsecured: 587 - secured
    /// </summary>
    public string SmtpPort { get; set; } = "25";

    /// <summary>
    /// Smtp Server User Id
    /// </summary>
    public string SmtpUserId { get; set; } = string.Empty;

    /// <summary>
    /// Smtp Server User Password
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Is the email in html format?
    /// Default: true
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// Message Importance - default Normal
    /// </summary>
    public MessageImportance Importance
    {
        get => _importance;
        set => _importance = value < MessageImportance.Low || value > MessageImportance.High
            ? MessageImportance.Normal
            : value;
    }

    /// <summary>
    /// Mail Priority - default Normal
    /// </summary>
    public MessagePriority Priority
    {
        get => _priority;
        set => _priority = value < MessagePriority.NonUrgent || value > MessagePriority.Urgent
            ? MessagePriority.Normal
            : value;
    }

    /// <summary>
    /// Use SSL Encryption When Sending Message
    /// Default: true
    /// </summary>
    public bool UseSSL { get; set; } = true;

    /// <summary>
    /// Gets or sets the directory path used for SMTP pickup.
    /// </summary>
    public string SmtpPickupDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP timeout in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 100_000;

    /// <summary>
    /// Gets or sets a value indicating whether invalid server certificates are accepted.
    /// Defaults to false for production safety.
    /// </summary>
    public bool AllowInvalidServerCertificate { get; set; }

    #endregion Public Properties

    #region Private Properties

    /// <summary>
    /// Backer for Importance
    /// Default: MessageImportance.Normal
    /// </summary>
    private MessageImportance _importance;

    /// <summary>
    /// Backer for MailPriority
    /// Default: MessagePriority.Normal
    /// </summary>
    private MessagePriority _priority;

    private int ResolvedSmtpPort { get; set; } = 25;

    private readonly List<Header> Headers = [];

    #endregion Private Properties

    #region Public Methods

    /// <summary>
    /// Create Email Attachment From String
    /// </summary>
    /// <param name="content">The content of the attachment as a string.</param>
    /// <param name="name">The name of the attachment file.</param>
    /// <param name="contentEncoding">The encoding of the content.</param>
    /// <param name="mediaType">The media type of the attachment.</param>
    public void CreateAttachmentFromString(string content, string name, Encoding contentEncoding, string mediaType)
    {
        if(string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Attachment name is required.", nameof(name));

        StringAttachments.Add(new StringAttachment
        {
            Content = content ?? string.Empty,
            Name = name,
            ContentEncoding = contentEncoding ?? Encoding.UTF8,
            MediaType = string.IsNullOrWhiteSpace(mediaType) ? "text/plain" : mediaType
        });
    }

    /// <summary>
    /// Clear All Recipient Lists
    /// </summary>
    public void ClearAllRecipients()
    {
        ToAddressList.Clear();          // Clear ToAddress List
        CCAddressList.Clear();          // Clear CCAddress List
        BCCAddressList.Clear();         // Clear BCCAddress List
        ReplyToAddressList.Clear();     // Clear ReplyToAddress List
    }

    /// <summary>
    /// Add Header To The Message
    /// </summary>
    /// <param name="name"> Header Name</param>
    /// <param name="value">Header Value</param>
    public void AddHeader(string name, string value)
    {
        if(string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value)) return;  // Don't Add Invalid Headers
        if(ContainsNewLine(name) || ContainsNewLine(value)) return;                      // Prevent header injection.

        Headers.Add(new Header
        {
            Name = name.Trim(),
            Value = value.Trim()
        });
    }

    /// <summary>
    /// Send the email.
    /// </summary>
    /// <returns>
    /// Tuple of bool, string
    /// bool:Success or Failure of email delivery.
    /// string: Associated Message
    /// </returns>
    public Tuple<bool, string> Send()
    {
        Tuple<bool, string> isValid = ValidateParameters(); // Are All Parameters Valid?

        if (!isValid.Item1) return isValid;                 // Bail On Invalid Parameters Supplied

        MimeMessage message;

        try
        {
            message = BuildMessage();                       // Build The Email Message
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException or NotSupportedException)
        {
            return new Tuple<bool, string>(false, $"Error while building email message: {e.Message}");
        }

        // If Email Sending Is Not Enabled, Write The Email To Disk Instead (If Valid Directory Supplied)
        if(!GetAppSetting("Email:SendEnabled", "true").StringToBoolean())             // Are We Just Testing? If Not Sending, Then Write Email To Disk Instead (If Valid Directory Supplied)
        {
            // Make sure pickup directory exists
            SmtpPickupDirectory = GetAppSetting("Email:SmtpPickupDirectory", SmtpPickupDirectory)!;   // Get Pickup Directory From App Settings If Not Supplied

            if(!string.IsNullOrWhiteSpace(SmtpPickupDirectory)) Directory.CreateDirectory(SmtpPickupDirectory);

            string pickupDirectory = string.IsNullOrWhiteSpace(SmtpPickupDirectory)
                ? AppContext.BaseDirectory
                : SmtpPickupDirectory;

            Directory.CreateDirectory(pickupDirectory);

            string path = Path.Combine(pickupDirectory, Guid.NewGuid().ToString("N") + ".eml");        // Build Email's File Name

            using(FileStream stream = new(path, FileMode.CreateNew))                           // Create File Stream For Email
            {
                message.WriteTo(stream);                                                                // Write Email To File
            }

            return new Tuple<bool, string>(true, $"Email written to {path}");                           // Return Success Message With File Path

        }

        // Create SmtpClient
        using SmtpClient smtpClient = new()
        {
            Timeout = TimeoutMilliseconds > 0 ? TimeoutMilliseconds : 100_000
        };

        if(AllowInvalidServerCertificate) smtpClient.ServerCertificateValidationCallback = (_, _, _, _) => true;

        // Connect to Smtp Server
        try
        {
            SecureSocketOptions secureSocketOptions = UseSSL
                ? SecureSocketOptions.StartTlsWhenAvailable
                : SecureSocketOptions.None;

            smtpClient.Connect(SmtpHost, ResolvedSmtpPort, secureSocketOptions);
        }
        catch (SmtpCommandException e)
        {
            return new Tuple<bool, string>(false, $"Error Code: {e.StatusCode} - {e.Message}");
        }
        catch (SmtpProtocolException e)
        {
            return new Tuple<bool, string>(false, $"Protocol error while trying to connect: {e.Message}");
        }
        catch(Exception e) when(e is IOException or TimeoutException or InvalidOperationException or System.Net.Sockets.SocketException)
        {
            return new Tuple<bool, string>(false, $"Error while trying to connect: {e.Message}");
        }

        // Note: Not all SMTP servers support authentication
        if (smtpClient.Capabilities.HasFlag(SmtpCapabilities.Authentication)
            && !string.IsNullOrWhiteSpace(SmtpUserId))
        {
            try
            {
                smtpClient.Authenticate(SmtpUserId, SmtpPassword);
            }
            catch (AuthenticationException)
            {
                return new Tuple<bool, string>(false, "Invalid user name or password.");
            }
            catch (SmtpCommandException e)
            {
                return new Tuple<bool, string>(false, $"Error trying to authenticate. Error Code: {e.StatusCode} - {e.Message}");
            }
            catch (SmtpProtocolException e)
            {
                return new Tuple<bool, string>(false, $"Protocol error while trying to authenticate: {e.Message}");
            }
        }

        string errorMessage = string.Empty;

        try
        {
            FormatOptions options = FormatOptions.Default.Clone();

            if (smtpClient.Capabilities.HasFlag(SmtpCapabilities.UTF8)) options.International = true;

            smtpClient.Send(options, message);
        }
        catch (SmtpCommandException e)
        {
            errorMessage = $"Error sending message. Error Code: {e.StatusCode} - {e.Message}";

            switch (e.ErrorCode)
            {
                case SmtpErrorCode.RecipientNotAccepted:
                    errorMessage += $" - Recipient not accepted: {e.Mailbox}";
                    break;

                case SmtpErrorCode.SenderNotAccepted:
                    errorMessage += $" - Sender not accepted: {e.Mailbox}";
                    break;

                case SmtpErrorCode.MessageNotAccepted:
                    errorMessage += " - Message not accepted.";
                    break;
            }
        }
        catch (SmtpProtocolException e)
        {
            errorMessage = $"Protocol error while sending message: {e.Message}";
        }
        catch (InvalidOperationException e)
        {
            errorMessage = $"Unexpected error while sending message: {e.Message}";
        }
        finally
        {
            if (smtpClient.IsConnected) smtpClient.Disconnect(true);
        }

        if (!string.IsNullOrEmpty(errorMessage)) return new Tuple<bool, string>(false, errorMessage);

        return new Tuple<bool, string>(true, "Success");
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// Remove All Recipients From The Message
    /// </summary>
    /// <param name="message">The message from which to clear recipients.</param>
    private static void ClearMessageRecipients(MimeMessage message)
    {
        message.To.Clear();         // Clear "To" Address List
        message.Cc.Clear();         // Clear "CC" Address List
        message.Bcc.Clear();        // Clear "BCC" Address List
        message.ReplyTo.Clear();    // Clear "ReplyTo" Address List
    }

    /// <summary>
    /// Format The Display Name For Each Address In The Supplied MailxAddesss List
    /// </summary>
    /// <param name="addresses">The list of addresses to format.</param>
    private static List<Tuple<string, string>> CheckDisplayName(List<Tuple<string, string>> addresses)
    {
        List<Tuple<string, string>> newAddresses = [];                                      // Create New List

        foreach (Tuple<string, string> item in addresses)                                   // Process Each Recipient
        {
            if(item == null) continue;

            string first = item.Item1?.Trim() ?? string.Empty;
            string second = item.Item2?.Trim() ?? string.Empty;
            string address;
            string displayName;

            if(first.IsValidEmailAddress())
            {
                address = first.ToLowerInvariant();
                displayName = string.IsNullOrWhiteSpace(second) ? address : second.FormatName()!;
            }
            else if(second.IsValidEmailAddress())
            {
                address = second.ToLowerInvariant();
                displayName = string.IsNullOrWhiteSpace(first) ? address : first.FormatName()!;
            }
            else
            {
                address = first.ToLowerInvariant();
                displayName = string.IsNullOrWhiteSpace(second) ? address : second.FormatName()!;
            }

            if (string.IsNullOrWhiteSpace(address)) continue;                               // Skip Invalid Recipient

            // No Display Name?
            if(string.IsNullOrWhiteSpace(displayName)) displayName = address;                // Use Email Address As Display Name

            newAddresses.Add(new Tuple<string, string>(displayName, address));               // Add Recipient To List
        }

        return newAddresses;                                                                // Return Valid Addresses
    }

    /// <summary>
    /// Build List Of Recipient Addresses. These Addresses Will Be
    /// Appended To The Email, When Override is True.
    /// </summary>
    private static string BuildAddressList(MimeMessage message)
    {
        string results = "<br /><br /><br /><strong>This message has been Redirected</strong>";

        if (message.To.Count > 0) results += "<br /><br />To List<br />" + message.To;                  // Add "To" Addresses
        if (message.Cc.Count > 0) results += "<br /><br />CC List<br />" + message.Cc;                  // Add "CC" Addresses
        if (message.Bcc.Count > 0) results += "<br /><br />BCC List<br />" + message.Bcc;               // Add "BCC" Addresses
        if (message.ReplyTo.Count > 0) results += "<br /><br />ReplyTo List<br />" + message.ReplyTo;   // Add "ReplyTo" Addresses

        return results;                                                                                 // Return Result String
    }

    /// <summary>
    /// Validate the supplied parameters before attempting to send email.
    /// </summary>
    /// <returns>
    /// Tuple of bool,string - bool:Success or Falure of Validation, string:Associated Message
    /// </returns>
    private Tuple<bool, string> ValidateParameters()
    {
        // Do We Have Anyone To Send The Message To?
        if (ToAddressList.Count == 0
            && CCAddressList.Count == 0
            && BCCAddressList.Count == 0)
            return new Tuple<bool, string>(false, "No recipients have been specified.");

        // Must have FromAddress
        FromAddress = FromAddress?.Trim().ToLowerInvariant() ?? string.Empty;
        FromName = string.IsNullOrWhiteSpace(FromName) ? FromAddress : FromName.Trim().FormatName()!;
        Subject = SanitizeHeaderValue(Subject);

        if (string.IsNullOrWhiteSpace(FromAddress)) return new Tuple<bool, string>(false, "FromAddress has not been specified.");
        if (!FromAddress.IsValidEmailAddress()) return new Tuple<bool, string>(false, $"FromAddress ({FromAddress}) is not a valid email format.");

        // Must have SMTP Server
        SmtpHost = SmtpHost?.Trim() ?? string.Empty;
        if(string.IsNullOrEmpty(SmtpHost)) SmtpHost = GetAppSetting("Email:SmtpHost")!;                          // Get SMTP Host Name

        SmtpHost = SmtpHost?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(SmtpHost)) return new Tuple<bool, string>(false, "SmtpHost has not been specified.");  // Return Error If No SMTP Host Supplied

        if(string.IsNullOrWhiteSpace(SmtpPort)) SmtpPort = GetAppSetting("Email:SmtpPort", "25")!;

        SmtpPort = SmtpPort.Trim();

        if(!int.TryParse(SmtpPort, out int smtpPort) || smtpPort is < 1 or > 65535)
            return new Tuple<bool, string>(false, $"SmtpPort ({SmtpPort}) is not a valid TCP port.");

        ResolvedSmtpPort = smtpPort;

        // If using port 587 we need a UserId and Password
        if (ResolvedSmtpPort == 587)
        {
            if(string.IsNullOrEmpty(SmtpUserId)) SmtpUserId = GetAppSetting("Email:SmtpUserId")!;               // Get SMTP Host Name
            if(string.IsNullOrEmpty(SmtpPassword)) SmtpPassword = GetAppSetting("Email:SmtpPassword")!;         // Get SMTP Password
            if (string.IsNullOrEmpty(SmtpUserId)) return new Tuple<bool, string>(false, "No 'UserID' specified");      // Need UserId For Port 587
            if (string.IsNullOrEmpty(SmtpPassword)) return new Tuple<bool, string>(false, "No 'Password' specified");  // Need Password For Port 587
        }

        // Format DisplayNames
        ToAddressList = CheckDisplayName(ToAddressList);            // Format "To" Display Names
        CCAddressList = CheckDisplayName(CCAddressList);            // Format "CC" Display Names
        BCCAddressList = CheckDisplayName(BCCAddressList);          // Format "BCC" Display Names
        ReplyToAddressList = CheckDisplayName(ReplyToAddressList);  // Format "ReplyTo" Display Names

        Tuple<bool, string> addressValidation = ValidateAddressList("To", ToAddressList);
        if (!addressValidation.Item1) return addressValidation;

        addressValidation = ValidateAddressList("CC", CCAddressList);
        if (!addressValidation.Item1) return addressValidation;

        addressValidation = ValidateAddressList("BCC", BCCAddressList);
        if (!addressValidation.Item1) return addressValidation;

        addressValidation = ValidateAddressList("ReplyTo", ReplyToAddressList);
        if (!addressValidation.Item1) return addressValidation;

        return new Tuple<bool, string>(true, "Success");            // Return Success
    }

    /// <summary>
    /// Validate the supplied list of addresses. Ensure each address is in a valid email format.
    /// </summary>
    /// <param name="listName">The name of the address list (e.g., "To", "CC").</param>
    /// <param name="addresses">The list of addresses to validate.</param>
    /// <returns>A tuple indicating whether the validation was successful and an associated message.</returns>
    private static Tuple<bool, string> ValidateAddressList(string listName, List<Tuple<string, string>> addresses)
    {
        foreach (Tuple<string, string> address in addresses)
        {
            if(address == null) continue;

            if (!address.Item2.IsValidEmailAddress())
                return new Tuple<bool, string>(false, $"{listName} address ({address.Item2}) is not a valid email format.");
        }

        return new Tuple<bool, string>(true, "Success");
    }

    /// <summary>
    /// Build The Message To Be Emailed
    /// </summary>
    private MimeMessage BuildMessage()
    {
        MimeMessage message = new()                // Create Message Object
        {
            Subject = Subject,                              // Set Message Subject
            Priority = Priority                             // Set Message Priority
        };

        message.From.Add(new MailboxAddress(FromName, FromAddress));

        // Add Message Recipients
        foreach (Header header in Headers) message.Headers.Add(header.Name, header.Value);                                                 // Add Any Additional Headers
        foreach (Tuple<string, string> item in ToAddressList) message.To.Add(new MailboxAddress(item.Item1, item.Item2));           // Add To Addresses
        foreach (Tuple<string, string> item in CCAddressList) message.Cc.Add(new MailboxAddress(item.Item1, item.Item2));           // Add CC Addresses
        foreach (Tuple<string, string> item in BCCAddressList) message.Bcc.Add(new MailboxAddress(item.Item1, item.Item2));         // Add BCC Addresses
        foreach (Tuple<string, string> item in ReplyToAddressList) message.ReplyTo.Add(new MailboxAddress(item.Item1, item.Item2)); // Add ReplyTo Addresses

        string? emailOverride = GetAppSetting("Email:Override");                // Check for Email Address Override

        if (emailOverride.IsValidEmailAddress())                                       // Has A Valid Override Email Address Been Supplied?
        {
            // Redirect All Email To This Address
            TextBody += BuildAddressList(message);                                     // Add All Current Recipients To End Of Message
            ClearMessageRecipients(message);                                           // Clear All Current Message Recipients
            message.To.Add(new MailboxAddress(emailOverride, emailOverride!));  // Add Override Address To Message
        }

        // Build The Message Body
        BodyBuilder builder = new()
        {
            TextBody = IsHtml ? StripHtmlFallback(TextBody) : TextBody,
            HtmlBody = IsHtml ? (string.IsNullOrWhiteSpace(HtmlBody) ? TextBody : HtmlBody) : null
        };

        foreach (string item in Attachments)
        {
            if(string.IsNullOrWhiteSpace(item)) continue;
            if(!File.Exists(item)) throw new FileNotFoundException("Email attachment was not found.", item);

            builder.Attachments.Add(item);                                      // Add File Attachments
        }

        // Add Inline Attachments
        foreach (InlineAttachment item in InlineAttachments)
        {
            if(string.IsNullOrWhiteSpace(item.FileName)) continue;
            if(!File.Exists(item.FileName)) throw new FileNotFoundException("Inline email attachment was not found.", item.FileName);

            MimeEntity image = builder.LinkedResources.Add(item.FileName);     // Add File To Message
            image.ContentId = string.IsNullOrWhiteSpace(item.CID) ? MimeUtils.GenerateMessageId() : item.CID; // Create ID For File

            //builder.linkedresources linkfile = new(item.filename)           // create linkedresource
            //{
            //    contentid = item.cid,                                          // give it an id
            //    transferencoding = contentencoding.base64                      // set mime type
            //};

            //htmlView.LinkedResources.Add(LinkFile);                            // Add the LinkedResource to the appropriate view
        }

        // Add String Attachments
        foreach (StringAttachment item in StringAttachments)
        {
            byte[] content = item.ContentEncoding.GetBytes(item.Content);
            builder.Attachments.Add(item.Name, content, ContentType.Parse(item.MediaType));
        }

        message.Body = builder.ToMessageBody();                                // Add Constructed Body To Message

        return message;                                                        // Return The Message
    }

    /// <summary>
    /// Strip HTML tags from the input string and decode HTML entities.
    /// This is a fallback method for stripping HTML when the input is expected to be in HTML format.
    /// </summary>
    /// <param name="value">The input string containing HTML.</param>
    /// <returns>The plain text string with HTML tags removed and HTML entities decoded.</returns>
    private static string StripHtmlFallback(string value)
    {
        if(string.IsNullOrWhiteSpace(value)) return string.Empty;

        string text = value.Replace("<br />", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("<br>", Environment.NewLine, StringComparison.OrdinalIgnoreCase);

        return WebUtility.HtmlDecode(HtmlTagRegex.Replace(text, string.Empty));
    }

    /// <summary>
    /// Check if the given string contains any newline characters (CR or LF).
    /// </summary>
    /// <param name="value">The string to check for newline characters.</param>
    /// <returns>True if the string contains any newline characters; otherwise, false.</returns>
    private static bool ContainsNewLine(string value) => value.Contains('\r') || value.Contains('\n');

    /// <summary>
    /// Sanitize the header value by replacing line endings with a space and trimming whitespace.
    /// </summary>
    /// <param name="value">The header value to sanitize.</param>
    /// <returns>The sanitized header value.</returns>
    private static string SanitizeHeaderValue(string? value) => string.IsNullOrWhiteSpace(value)
        ? string.Empty
        : value.ReplaceLineEndings(" ").Trim();
}

#endregion Private Methods

#region Sub Classes

/// <summary>
/// Define Inline Email Attachment
/// </summary>
public class InlineAttachment(string filename, string cid)
{
    /// <summary>
    /// Gets or sets the file name of the attachment.
    /// </summary>
    public string FileName { get; set; } = filename;
    /// <summary>
    /// Gets or sets the CID.
    /// </summary>
    public string CID { get; set; } = cid;
}

/// <summary>
/// Define String Email Attachment
/// </summary>
public class StringAttachment
{
    /// <summary>
    /// The text content.
    /// </summary>
    public required string Content { get; set; }
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the character encoding used for the content.
    /// </summary>
    public required Encoding ContentEncoding { get; set; }
    /// <summary>
    /// Media Type of the attachment. For example "text/plain" or "application/pdf"
    /// </summary>
    public required string MediaType { get; set; }
}

/// <summary>
/// Define Html Header Name/Value Pair
/// </summary>
public class Header
{
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public required string Value { get; set; }
}

/// <summary>
/// Provides an SMTP client implementation that supports Delivery Status Notification (DSN) configuration.
/// </summary>
/// <remarks>This implementation configures the client to use the message's Message-Id as the envelope identifier
/// and requests failure notifications for all recipients. The envelope identifier allows correlation of delivery status
/// notifications with the original message.</remarks>
public class DSNSmtpClient : SmtpClient
{
    /// <summary>
    /// Get the envelope identifier to be used with delivery status notifications.
    /// </summary>
    /// <remarks>
    /// <para>The envelope identifier, if non-empty, is useful in determining which message
    /// a delivery status notification was issued for.</para>
    /// <para>The envelope identifier should be unique and may be up to 100 characters in
    /// length, but must consist only of printable ASCII characters and no white space.</para>
    /// <para>For more information, see rfc3461, section 4.4.</para>
    /// </remarks>
    /// <returns>The envelope identifier.</returns>
    /// <param name="message">The message.</param>
    protected override string GetEnvelopeId(MimeMessage message) =>
        // Since you will want to be able to map whatever identifier you return here to the
        // message, the obvious identifier to use is probably the Message-Id value.
        message.MessageId!;

    /// <summary>
    /// Get the types of delivery status notification desired for the specified recipient mailbox.
    /// </summary>
    /// <returns>The desired delivery status notification type.</returns>
    /// <param name="message">The message being sent.</param>
    /// <param name="mailbox">The mailbox.</param>
    protected override DeliveryStatusNotification? GetDeliveryStatusNotifications(MimeMessage message, MailboxAddress mailbox) =>
        // In this example, we only want to be notified of failures to deliver to a mailbox.
        // If you also want to be notified of delays or successful deliveries, simply bitwise-or
        // whatever combination of flags you want to be notified about.
        DeliveryStatusNotification.Failure;

        //DeliveryStatusNotification.Delay;
        //DeliveryStatusNotification.Failure;
        //DeliveryStatusNotification.Never;
        //DeliveryStatusNotification.Success;
}

#endregion Sub Classes