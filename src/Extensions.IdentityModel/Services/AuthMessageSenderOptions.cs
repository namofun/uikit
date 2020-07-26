namespace SatelliteSite.IdentityModule.Services
{
    public class AuthMessageSenderOptions
    {
        public string Sender { get; set; }

        public string User { get; set; }

        public string Key { get; set; }

        public string Server { get; set; }

        public int Port { get; set; } = 465;
    }
}
