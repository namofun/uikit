namespace Microsoft.Extensions.Mailing
{
    public class SmtpEmailSenderOptions
    {
        public string Sender { get; set; } = string.Empty;

        public string User { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Server { get; set; } = string.Empty;

        public int Port { get; set; } = 465;
    }
}
