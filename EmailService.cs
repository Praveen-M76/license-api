using System.Net;
using System.Net.Mail;

namespace LicenseApi
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var fromEmail = "praveenmathu20@gmail.com"; // your gmail
            var appPassword = "ivaeqxaaasktnyoo"; // your app password (no spaces)

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true,
                Timeout = 20000
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            await smtp.SendMailAsync(message);
        }
    }
}
