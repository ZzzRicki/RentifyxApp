using Azure;
using Azure.Communication.Email;
using Rentifyx.BLL.Contract;

public class EmailService : IEmailService
{
    private readonly EmailClient _emailClient;

    public EmailService(string connectionString)
    {
        _emailClient = new EmailClient(connectionString);
    }

    public async Task<bool> SendEmailAsync(string toAddress, string subject, string htmlBody)
    {
        string senderAddress = "DoNotReply@rentifyx.aerylab.com";

        var emailMessage = new EmailMessage(
            senderAddress: senderAddress,
            content: new EmailContent(subject)
            {
                Html = htmlBody
            },
            recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(toAddress) })
        );

        try
        {
            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

            if (emailSendOperation.HasCompleted)
            {
                Console.WriteLine("Email sent successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Failed to send email.");
                return false;
            }
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendSupportEmailAsync(string toAddress, string subject, string htmlBody)
    {
        string senderAddress = "support@rentifyx.aerylab.com";

        var emailMessage = new EmailMessage(
            senderAddress: senderAddress,
            content: new EmailContent(subject)
            {
                Html = htmlBody
            },
            recipients: new EmailRecipients(new List<EmailAddress> { new EmailAddress(toAddress) })
        );

        try
        {
            EmailSendOperation emailSendOperation = await _emailClient.SendAsync(WaitUntil.Completed, emailMessage);

            if (emailSendOperation.HasCompleted)
            {
                Console.WriteLine("Email sent successfully.");
                return true;
            }
            else
            {
                Console.WriteLine("Failed to send email.");
                return false;
            }
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Email sending failed: {ex.Message}");
            return false;
        }
    }
}
