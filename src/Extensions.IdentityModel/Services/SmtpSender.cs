using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.Services
{
    public class SmtpSender : IEmailSender
    {
        private ILogger<SmtpSender> Logger { get; }
        public AuthMessageSenderOptions ApiKey { get; set; }

        public SmtpSender(ILogger<SmtpSender> logger, IOptions<AuthMessageSenderOptions> options)
        {
            Logger = logger;
            ApiKey = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var msg = new MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress(ApiKey.User, ApiKey.Sender);

            msg.Subject = subject;
            msg.SubjectEncoding = Encoding.UTF8;

            msg.IsBodyHtml = true;
            msg.Body = message;
            msg.BodyEncoding = Encoding.UTF8;

            var client = new SmtpClient
            {
                Host = ApiKey.Server,
                Port = ApiKey.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(ApiKey.User, ApiKey.Key)
            };

            Logger.LogInformation("An email will be sent to {Email}", email);
            Task.Run(async () => await client.SendMailAsync(msg));
            return Task.CompletedTask;
        }
    }
}
