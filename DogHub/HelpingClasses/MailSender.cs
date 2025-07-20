using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace DogHub.HelpingClasses
{
    public class MailSender
    {
        private const string FromName = "DogHub Support";
        private const string SmtpHost = "smtp.gmail.com";
        private static readonly string FromEmail = ConfigurationManager.AppSettings["SmtpEmail"];
        private static readonly string Password = ConfigurationManager.AppSettings["SmtpPassword"];
        private const int SmtpPort = 587;

        public static bool SendForgotPasswordEmail(string toEmail, string resetLink)
        {
            string subject = "Reset Your Password";
            string body = $@"
                <html><body>
                <h2>Password Reset</h2>
                <p>To reset your password, click the button below. (Note: This link expires after 24 hours)</p>
                <a href='{resetLink}' style='padding:10px 20px; background:#00aeef; color:white; text-decoration:none;'>Reset Password</a>
                <p>If you did not request this, you can ignore this email.</p>
                </body></html>";

            return SendEmail(toEmail, subject, body);
        }

        public static bool SendEmailToAdmin(string userName, string subject, string userEmail, string adminEmail, string message, string baseUrl = "")
        {
            string mailBody = $@"
                <html><body>
                <h2>Message from DogHub User</h2>
                <p><strong>Name:</strong> {userName}</p>
                <p><strong>Email:</strong> {userEmail}</p>
                <p><strong>Message:</strong><br>{message}</p>
                <hr>
                <a href='{baseUrl}Home/Index' style='padding:10px 20px; background:#00aeef; color:white; text-decoration:none;'>Visit DogHub</a>
                </body></html>";

            return SendEmail(adminEmail, subject ?? "New Message from User", mailBody);
        }

        public static bool SendEmailToUser(string userEmail, string adminName, string adminEmail, string baseUrl = "")
        {
            string mailBody = $@"
                <html><body>
                <h2>Message from Admin</h2>
                <p>Thank you for contacting DogHub!</p>
                <p>If you have more queries, feel free to reach out again.</p>
                <br />
                <p>Best regards,</p>
                <p>{adminName}<br />{adminEmail}<br />DogHub Team</p>
                <hr>
                <a href='{baseUrl}Home/Index' style='padding:10px 20px; background:#00aeef; color:white; text-decoration:none;'>Visit DogHub</a>
                </body></html>";

            return SendEmail(userEmail, "Message from DogHub Admin", mailBody);
        }

        private static bool SendEmail(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var fromAddress = new MailAddress(FromEmail, FromName);
                var toAddress = new MailAddress(toEmail);
                var smtp = new SmtpClient
                {
                    Host = SmtpHost,
                    Port = SmtpPort,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(FromEmail, Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 20000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(message);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
