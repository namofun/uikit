using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SatelliteSite.AzureCloud
{
    public class EasyAuthModule : BaseAuthModule
    {
        public override string Area => string.Empty;

        public override void Initialize()
        {
        }

        protected override void RegisterOtherServices(IServiceCollection services)
        {
            services.Configure<SubstrateOptions>(options =>
            {
                options.LoginRouteName = "EasyAuthLogin";
                options.LogoutRouteName = "EasyAuthLogout";
            });
        }

        protected override void BuildAuthentication(AuthenticationBuilder builder)
        {
            builder.AddEasyAuth();
        }
    }
}
