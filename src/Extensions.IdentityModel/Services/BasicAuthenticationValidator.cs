using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    public class BasicAuthenticationValidator : BasicAuthenticationEvents
    {
        public BasicAuthenticationValidator()
        {
            OnValidateCredentials = ValidateAsync;
        }

        private readonly static IMemoryCache _cache =
            new MemoryCache(new MemoryCacheOptions()
            {
                Clock = new Microsoft.Extensions.Internal.SystemClock()
            });

        private static async Task ValidateAsync(ValidateCredentialsContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Username))
            {
                context.Fail("User not found.");
                return;
            }

            var userManager = context.HttpContext.RequestServices.GetRequiredService<IUserManager>();
            var normusername = userManager.NormalizeName(context.Username);
            var user = await _cache.GetOrCreateAsync("`" + normusername, async entry =>
            {
                var value = await userManager.FindByNameAsync(context.Username);
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return value;
            });

            if (user == null)
            {
                context.Fail("User not found.");
                return;
            }

            var attempt = userManager.VerifyPassword(user, context.Password);
            if (attempt == PasswordVerificationResult.Failed)
            {
                context.Fail("Login failed, password not match.");
                return;
            }

            var principal = await _cache.GetOrCreateAsync(normusername, async entry =>
            {
                var ur = await userManager.GetRolesAsync(user);
                var options = userManager.Options;

                var id = new ClaimsIdentity(IdentityConstants.ApplicationScheme,
                    options.ClaimsIdentity.UserNameClaimType,
                    options.ClaimsIdentity.RoleClaimType);
                id.AddClaim(new Claim(options.ClaimsIdentity.UserIdClaimType, $"{user.Id}"));
                id.AddClaim(new Claim(options.ClaimsIdentity.UserNameClaimType, user.UserName));
                id.AddClaim(new Claim(options.ClaimsIdentity.SecurityStampClaimType, user.SecurityStamp));
                foreach (var roleName in ur)
                    id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, roleName));
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20);
                return new ClaimsPrincipal(id);
            });

            context.Principal = principal;
            context.Success();
        }
    }
}
