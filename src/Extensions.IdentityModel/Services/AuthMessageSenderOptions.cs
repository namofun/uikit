namespace SatelliteSite.Services
{
    public class AuthMessageSenderOptions
    {
        public string Sender { get; set; } = "admin";

        public string User { get; set; } = "admin@admin.com";

        public string Key { get; set; } = string.Empty;

        public string Server { get; set; } = "smtp.exmail.qq.com";

        public int Port { get; set; } = 465;
    }
}
