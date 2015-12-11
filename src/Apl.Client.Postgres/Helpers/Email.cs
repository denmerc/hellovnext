using System.Net;
using System.Net.Mail;

namespace APLPX.Client.Postgres.Helpers
{
    public class Email
    {
        public static void Send(  
                                string sendFromAddress,
                                string sendToAddress,
                                string subject, 
                                string body, 
                                bool isBodyHtml,
                                string smtpClientHost,
                                int smtpClientPort,
                                string smtpClientUser,
                                string smtpClientPassword,
                                bool isSSLEnabled       )
        {

            var fromAddress = new MailAddress(sendFromAddress);
            var toAddress = new MailAddress(sendToAddress);

            using(var smtp = new SmtpClient
                {
                    Host = smtpClientHost,
                    Port = smtpClientPort,
                    EnableSsl = isSSLEnabled,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpClientUser, smtpClientPassword)
                })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                })
                {
                    smtp.Send(message);
                }
            }
        }

        public static void Send(
                                string sendFromAddress,
                                string sendToAddress,
                                string subject,
                                string body,
                                bool isBodyHtml,
                                SmtpClient smtpClient)
        {

            var fromAddress = new MailAddress(sendFromAddress);
            var toAddress = new MailAddress(sendToAddress);

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            })
            {
                smtpClient.Send(message);
            }
        }

    }
}
