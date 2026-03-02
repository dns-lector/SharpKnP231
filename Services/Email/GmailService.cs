using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace SharpKnP321.Services.Email
{
    internal class GmailService : IEmailService
    {
        private JsonElement? gmailSection;

        public async Task SendAsync(MailMessage mailMessage)
        {
            if(gmailSection is null)
            {
                String settingsFilename = "appsettings.json";
                if (!File.Exists(settingsFilename))
                {
                    Console.WriteLine("Не знайдено файл конфігурації. Прочитайте Readme");
                    return;
                }
                var settings = JsonSerializer.Deserialize<JsonElement>(
                    File.ReadAllText(settingsFilename)
                );
                var emailSection = settings.GetProperty("Emails");
                gmailSection = emailSection.GetProperty("Gmail");
                if(gmailSection is null)
                {
                    throw new Exception("Помилка визначення конфігурації");
                }
            }
            using SmtpClient smtpClient = new()
            {
                Host = gmailSection!.Value.GetProperty("Server").GetString()!,
                Port = gmailSection!.Value.GetProperty("Port").GetInt32(),
                EnableSsl = gmailSection!.Value.GetProperty("Ssl").GetBoolean(),
                Credentials = new NetworkCredential(
                    gmailSection!.Value.GetProperty("Username").GetString()!, 
                    gmailSection!.Value.GetProperty("Password").GetString()!
                ),
            };
            mailMessage.From = new MailAddress(gmailSection!.Value.GetProperty("Username").GetString()!);
            smtpClient.Send(mailMessage);
        }
    }
}
