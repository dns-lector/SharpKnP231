using System.Net.Mail;

namespace SharpKnP321.Services.Email
{
    internal interface IEmailService
    {
        public Task SendAsync(MailMessage mailMessage);
    }
}
