using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MimeKit;

namespace FCS.Utilities.UnitTests;

[TestClass]
public sealed class EmailHelperTests
{
    [TestMethod]
    public void Send_WhenFromAddressIsInvalid_ReturnsValidationFailure()
    {
        EmailHelper email = new()
        {
            FromAddress = "not-an-address",
            SmtpHost = "localhost",
            Subject = "Validation test",
            TextBody = "Body",
            IsHtml = false
        };

        email.ToAddressList.Add(new Tuple<string, string>("recipient@example.com", "Recipient"));

        Tuple<bool, string> result = email.Send();

        Assert.IsFalse(result.Item1);
        StringAssert.Contains(result.Item2, "FromAddress");
    }

    [TestMethod]
    public void Send_WhenPickupIsEnabled_WritesValidMessageAndNormalizesRecipients()
    {
        string pickupDirectory = Path.Combine(Path.GetTempPath(), "FCS.Utilities.EmailHelperTests", Guid.NewGuid().ToString("N"));

        try
        {
            EmailHelper email = new()
            {
                FromAddress = "SENDER@EXAMPLE.COM",
                FromName = "test sender",
                SmtpHost = "localhost",
                SmtpPort = "25",
                SmtpPickupDirectory = pickupDirectory,
                Subject = "Pickup test",
                TextBody = "Plain text body",
                HtmlBody = "<strong>HTML body</strong>",
                IsHtml = true
            };

            // Supports either tuple order for compatibility with existing callers.
            email.ToAddressList.Add(new Tuple<string, string>("Recipient Person", "RECIPIENT@EXAMPLE.COM"));
            email.CCAddressList.Add(new Tuple<string, string>("copy@example.com", "copy person"));
            email.CreateAttachmentFromString("attachment content", "note.txt", Encoding.UTF8, "text/plain");

            Tuple<bool, string> result = email.Send();

            Assert.IsTrue(result.Item1, result.Item2);

            string emlPath = Directory.EnumerateFiles(pickupDirectory, "*.eml").Single();
            MimeMessage message = MimeMessage.Load(emlPath);

            MailboxAddress to = AssertSingleMailbox(message.To, "To");
            MailboxAddress cc = AssertSingleMailbox(message.Cc, "CC");

            Assert.AreEqual("recipient@example.com", to.Address);
            Assert.AreEqual("Recipient Person", to.Name);
            Assert.AreEqual("copy@example.com", cc.Address);
            Assert.AreEqual("Copy Person", cc.Name);
            Assert.AreEqual("sender@example.com", ((MailboxAddress)message.From.Single()).Address);
            Assert.AreEqual("Test Sender", ((MailboxAddress)message.From.Single()).Name);
            Assert.AreEqual(1, message.Attachments.Count());
        }
        finally
        {
            if(Directory.Exists(pickupDirectory)) Directory.Delete(pickupDirectory, recursive: true);
        }
    }

    [TestMethod]
    public void Send_WhenSmtpPortIsInvalid_ReturnsValidationFailure()
    {
        EmailHelper email = new()
        {
            FromAddress = "sender@example.com",
            SmtpHost = "localhost",
            SmtpPort = "invalid",
            Subject = "Invalid port test",
            TextBody = "Body",
            IsHtml = false
        };

        email.ToAddressList.Add(new Tuple<string, string>("recipient@example.com", "Recipient"));

        Tuple<bool, string> result = email.Send();

        Assert.IsFalse(result.Item1);
        StringAssert.Contains(result.Item2, "SmtpPort");
    }

    [TestMethod]
    public void Send_WhenAttachmentIsMissing_ReturnsValidationFailureInsteadOfThrowing()
    {
        EmailHelper email = new()
        {
            FromAddress = "sender@example.com",
            SmtpHost = "localhost",
            SmtpPickupDirectory = Path.Combine(Path.GetTempPath(), "FCS.Utilities.EmailHelperTests", Guid.NewGuid().ToString("N")),
            Subject = "Missing attachment test",
            TextBody = "Body",
            IsHtml = false
        };

        email.ToAddressList.Add(new Tuple<string, string>("recipient@example.com", "Recipient"));
        email.Attachments.Add(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.txt"));

        Tuple<bool, string> result = email.Send();

        Assert.IsFalse(result.Item1);
        StringAssert.Contains(result.Item2, "Error while building email message");
    }

    [TestMethod]
    public void Send_WhenSubjectContainsNewLine_WritesSanitizedSubject()
    {
        string pickupDirectory = Path.Combine(Path.GetTempPath(), "FCS.Utilities.EmailHelperTests", Guid.NewGuid().ToString("N"));

        try
        {
            EmailHelper email = new()
            {
                FromAddress = "sender@example.com",
                SmtpHost = "localhost",
                SmtpPickupDirectory = pickupDirectory,
                Subject = "Hello\r\nInjected: bad",
                TextBody = "Body",
                IsHtml = false
            };

            email.ToAddressList.Add(new Tuple<string, string>("recipient@example.com", "Recipient"));

            Tuple<bool, string> result = email.Send();

            Assert.IsTrue(result.Item1, result.Item2);

            MimeMessage message = MimeMessage.Load(Directory.EnumerateFiles(pickupDirectory, "*.eml").Single());
            Assert.AreEqual("Hello  Injected: bad", message.Subject);
        }
        finally
        {
            if(Directory.Exists(pickupDirectory)) Directory.Delete(pickupDirectory, recursive: true);
        }
    }

    private static MailboxAddress AssertSingleMailbox(InternetAddressList addresses, string listName)
    {
        Assert.AreEqual(1, addresses.Count, $"Expected one {listName} recipient.");
        Assert.IsInstanceOfType<MailboxAddress>(addresses[0]);

        return (MailboxAddress)addresses[0];
    }
}