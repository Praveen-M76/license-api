using System;
using System.Net;
using System.Net.Mail;

namespace LicenseApi
{
    public class EmailService
    {
        public bool SendVerificationCode(string toEmail, string code, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var fromEmail = "a98035001@smtp-brevo.com";
               var smtpPassword = Environment.GetEnvironmentVariable("SMTP_KEY");

                if (string.IsNullOrWhiteSpace(smtpPassword))
                {
                    errorMessage = "SMTP_KEY is missing in Railway Variables.";
                    return false;
                }

                using (var client = new SmtpClient("smtp-relay.brevo.com", 587))
                {
                    client.Credentials = new NetworkCredential(fromEmail, smtpPassword);
                    client.EnableSsl = true;
                    client.Timeout = 60000;

                    var mail = new MailMessage();
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(toEmail);
                    mail.Subject = "Your Trial Verification Code";
                    mail.Body = "Your verification code is: " + code;

                    client.Send(mail);
                }

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
