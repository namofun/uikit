using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SatelliteSite.Entities;
using System;
using System.Linq;
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
            var dbContext = context.HttpContext.RequestServices
                .GetRequiredService<IUserStore<User>>();
            if (!(dbContext is IQueryableUserStore<User> uqstore) || !(dbContext is IUserRoleStore<User> urstore))
                throw new InvalidOperationException("Using BasicAuthenticationValidator against invalid store.");

            var normusername = context.Username.ToUpper();

            var user = await _cache.GetOrCreateAsync("`" + normusername.ToLower(), async entry =>
            {
                var value = await uqstore.Users
                    .Where(u => u.NormalizedUserName == normusername)
                    .Select(u => new { u.Id, u.UserName, u.PasswordHash, u.SecurityStamp })
                    .FirstOrDefaultAsync();
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                return value;
            });

            if (user == null)
            {
                context.Fail("User not found.");
                return;
            }

            var passwordHasher = context.HttpContext.RequestServices
                .GetRequiredService<IPasswordHasher<User>>();

            var attempt = passwordHasher.VerifyHashedPassword(
                user: default, // assert that hasher don't need TUser
                hashedPassword: user.PasswordHash,
                providedPassword: context.Password);

            if (attempt == PasswordVerificationResult.Failed)
            {
                context.Fail("Login failed, password not match.");
                return;
            }

            var principal = await _cache.GetOrCreateAsync(normusername, async entry =>
            {
                // Assert the UserStore implemention doesn't use other properties
                var ur = await urstore.GetRolesAsync(new User { Id = user.Id }, default);

                var options = context.HttpContext.RequestServices
                    .GetRequiredService<IOptions<IdentityOptions>>().Value;

                // REVIEW: Used to match Application scheme
                var id = new ClaimsIdentity("Identity.Application",
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
