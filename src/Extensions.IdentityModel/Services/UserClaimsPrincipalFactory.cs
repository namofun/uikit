using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    public class UserClaimsPrincipalFactory<TUser, TRole, TContext> :
        UserClaimsPrincipalFactory<TUser>
        where TUser : Entities.User, new()
        where TRole : Entities.Role, new()
        where TContext : IdentityDbContext<TUser, TRole, int>
    {
        public IdentityDbContext<TUser, TRole, int> Identity { get; }

        public UserClaimsPrincipalFactory(
            UserManager<TUser> userManager,
            TContext identityDbContext,
            IOptions<IdentityOptions> options) :
            base(userManager, options)
        {
            Identity = identityDbContext;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser user)
        {
            var content = await base.GenerateClaimsAsync(user);

            var roleQuery =
                from ur in Identity.UserRoles
                where ur.UserId == user.Id
                join r in Identity.Roles on ur.RoleId equals r.Id
                select r.Name;

            var roles = await roleQuery.ToListAsync();
            content.AddClaims(roles.Select(roleName => new Claim(Options.ClaimsIdentity.RoleClaimType, roleName)));

            var roleClaimsQuery =
                from ur in Identity.UserRoles
                where ur.UserId == user.Id
                join r in Identity.RoleClaims on ur.RoleId equals r.RoleId                
                select new { r.ClaimType, r.ClaimValue };

            var roleClaims = await roleClaimsQuery.Distinct().ToListAsync();
            content.AddClaims(roleClaims.Select(r => new Claim(r.ClaimType, r.ClaimValue)));

            if (!string.IsNullOrWhiteSpace(user.NickName))
                content.AddClaim(new Claim("nickname", user.NickName));

            return content;
        }
    }
}
