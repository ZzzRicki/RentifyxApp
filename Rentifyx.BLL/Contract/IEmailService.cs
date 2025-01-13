namespace Rentifyx.BLL.Contract
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toAddress, string subject, string htmlBody);
        Task<bool> SendSupportEmailAsync(string toAddress, string subject, string htmlBody);
    }
}
