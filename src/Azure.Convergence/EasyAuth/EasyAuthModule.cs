using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace SatelliteSite.AzureCloud
{
    public class EasyAuthModule : BaseAuthModule
    {
        public override string Area => string.Empty;

        public override void Initialize()
        {
        }

        protected override void BuildAuthentication(AuthenticationBuilder builder)
        {
            builder.AddEasyAuth();
        }
    }
}
