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
                var fromEmail = "praveenmathu20@gmail.com";   // your verified sender
                var smtpLogin = "a98035001@smtp-brevo.com";   // from Brevo

                var smtpPassword = Environment.GetEnvironmentVariable("SMTP_KEY");

                if (string.IsNullOrWhiteSpace(smtpPassword))
                {
                    errorMessage = "SMTP_KEY missing in Railway.";
                    return false;
                }

                using (var client = new SmtpClient("smtp-relay.brevo.com", 2525)) // ✅ IMPORTANT
                {
                    client.Credentials = new NetworkCredential(smtpLogin, smtpPassword);
                    client.EnableSsl = true;
                    client.Timeout = 100000;

                    var mail = new MailMessage();
                    mail.From = new MailAddress(fromEmail, "IGCompareTool");
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
