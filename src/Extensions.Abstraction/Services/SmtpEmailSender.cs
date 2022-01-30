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
        private readonly Action<ILogger, string, Exception?> _emailSending;
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

            _emailSending = LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(17362),
                formatString: "An email will be sent to {email}");
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

            var client = new SmtpClient
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
                var startTime = DateTimeOffset.Now;
                Exception? exception = null;

                try
                {
                    await client.SendMailAsync(msg);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                telemetry.TrackDependency(
                    dependencyTypeName: "Smtp",
                    dependencyName: $"{client.Host}:{client.Port}",
                    target: msg.From.Address,
                    data: string.Join(';', msg.To.Select(a => a.Address)),
                    startTime: startTime,
                    duration: DateTimeOffset.Now - startTime,
                    resultCode: exception == null ? "OK" : exception.Message,
                    success: exception == null);
            });

            return Task.CompletedTask;
        }
    }
}
