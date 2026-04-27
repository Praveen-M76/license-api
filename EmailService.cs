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
                var fromEmail = "praveenmathu20@gmail.com";
                var appPassword = "vlabotqvdjhjotzm";

                using (var client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.Credentials = new NetworkCredential(fromEmail, appPassword);
                    client.EnableSsl = true;

                    var mail = new MailMessage();
                    mail.From = new MailAddress(fromEmail);
                    mail.To.Add(toEmail);
                    mail.Subject = "Your Trial Verification Code";
                    mail.Body =
                        "Your verification code is: " + code + "\n\n" +
                        "Use this code to claim your 7-day free trial.\n" +
                        "This code expires in 10 minutes.";

                    client.Send(mail);
                }

                return true;
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}