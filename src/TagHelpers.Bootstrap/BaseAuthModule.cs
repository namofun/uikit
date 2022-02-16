using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    public abstract class BaseAuthModule : AbstractModule
    {
        public sealed override bool ProvideIdentity => true;

        public sealed override void RegisterServices(IServiceCollection services)
        {
            BuildAuthentication(services.AddAuthentication());
            services.AddAuthorization();

            services.ConfigureOptions<AuthorizationPolicyRegistryConfigurator>();

            RegisterOtherServices(services);
        }

        public sealed override void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            base.RegisterServices(services, configuration);
        }

        public sealed override void RegisterServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            base.RegisterServices(services, configuration, environment);
        }

        protected virtual void BuildAuthentication(AuthenticationBuilder builder)
        {
        }

        protected virtual void RegisterOtherServices(IServiceCollection services)
        {
        }
    }

    internal interface IIdentityModuleOptions
    {
        bool EnableBasicAuthentication { get; set; }
    }
}
