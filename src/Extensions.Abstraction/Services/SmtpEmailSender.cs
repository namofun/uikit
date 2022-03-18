using Microsoft.Extensions.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Mailing
{
    public class SmtpEmailSender : IEmailSender
    {
        private static readonly Action<ILogger, string, Exception?>
            _emailSending = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(17362),
                formatString: "An email will be sent to {email}");

        private readonly ILogger<SmtpEmailSender> _logger;
        private readonly ITelemetryClient _telemetry;
        private readonly SmtpEmailSenderOptions _apikey;

        public SmtpEmailSender(
            ILogger<SmtpEmailSender> logger,
            ITelemetryClient telemetry,
            IOptions<SmtpEmailSenderOptions> options)
        {
            _logger = logger;
            _telemetry = telemetry;
            _apikey = options.Value;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var msg = new MailMessage();
            msg.To.Add(email);
            msg.From = new MailAddress(_apikey.User, _apikey.Sender);

            msg.Subject = subject;
            msg.SubjectEncoding = Encoding.UTF8;

            msg.IsBodyHtml = true;
            msg.Body = message;
            msg.BodyEncoding = Encoding.UTF8;

            using SmtpClient client = new()
            {
                Host = _apikey.Server,
                Port = _apikey.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(_apikey.User, _apikey.Key)
            };

            _emailSending(_logger, email, null);
            var telemetry = _telemetry;

            Task.Run(async () =>
            {
                using var tracker = telemetry.StartOperation("Queue Message | Smtp", $"{client.Host}:{client.Port}", "SendMail");
                tracker.Data = $"From: {msg.From.Address}\r\nTo: {string.Join(';', msg.To.Select(a => a.Address))}\r\n";
                try
                {
                    await client.SendMailAsync(msg);
                    tracker.ResultCode = "OK";
                }
                catch (Exception ex)
                {
                    tracker.Success = false;
                    tracker.ResultCode = ex.Message;
                }
                finally
                {
                    telemetry.StopOperation(tracker);
                }
            });

            return Task.CompletedTask;
        }
    }
}
