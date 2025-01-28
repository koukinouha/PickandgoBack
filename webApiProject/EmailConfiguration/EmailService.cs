using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace webApiProject.EmailConfiguration
{
    public class EmailService : IEmailService
    {

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("nouhe.kouki.1999@gmail.com", "oxqe nmhi bhln xpuo");

                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress("nouhe.kouki.1999@gmail.com");
                        mailMessage.To.Add(email);
                        mailMessage.Subject = subject;
                        mailMessage.Body = message;

                        await client.SendMailAsync(mailMessage);
                    }
                }
            }
            catch (SmtpException ex)
            {

                Console.WriteLine($"SMTP Error: {ex.Message}");
                throw;
            }
        }
    }
}