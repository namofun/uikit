using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SatelliteSite.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    public class UserManager : UserManager<User>
    {
        public UserManager(
            DefaultContext defaultContext,
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IEnumerable<IUserValidator<User>> userValidators,
            IEnumerable<IPasswordValidator<User>> passwordValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager> logger)
            : base(store,
                  optionsAccessor,
                  passwordHasher,
                  userValidators,
                  passwordValidators,
                  keyNormalizer,
                  errors,
                  services,
                  logger)
        {
            Context = defaultContext;
        }

        private DefaultContext Context { get; }
        public override bool SupportsUserTwoFactor => false;
        public override bool SupportsUserTwoFactorRecoveryCodes => false;

        public override string GetUserId(ClaimsPrincipal principal)
        {
            // Be careful of the old logins
            return base.GetUserId(principal)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public string GetNickName(ClaimsPrincipal claim)
        {
            var nickName = claim.FindFirstValue("nickname");
            if (string.IsNullOrEmpty(nickName)) nickName = GetUserName(claim);
            return nickName;
        }

        private Task<IdentityResult> SlideExpirationAsync(User user)
        {
            CookieAuthenticationValidator._cache.Set(
                key: "SlideExpiration: " + user.NormalizedUserName,
                value: DateTimeOffset.UtcNow,
                absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(20));
            return Task.FromResult(IdentityResult.Success);
        }

        public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            var result = await base.AddToRoleAsync(user, role);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        public override async Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles)
        {
            var result = await base.AddToRolesAsync(user, roles);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        public override async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
        {
            var result = await base.RemoveFromRoleAsync(user, role);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        public override async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
        {
            var result = await base.RemoveFromRolesAsync(user, roles);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        public override async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            var result = await base.ConfirmEmailAsync(user, token);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        public virtual Task<User> FindByIdAsync(int uid)
        {
            return FindByIdAsync($"{uid}");
        }

        public virtual Task<List<Role>> ListRolesAsync(User user)
        {
            var query = from userRole in Context.UserRoles
                        join role in Context.Roles on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(user.Id)
                        select role;
            return query.ToListAsync();
        }

        public virtual async Task<PagedViewList<User>> ListUsersAsync(int page, int pageCount)
        {
            var lst = await Users.OrderBy(u => u.Id)
                .Skip((page - 1) * pageCount).Take(pageCount)
                .ToListAsync();
            var count = await Users.CountAsync();
            return new PagedViewList<User>(lst, page, count, pageCount);
        }

        public virtual Task<List<IdentityUserRole<int>>> ListUserRolesAsync(int minUid, int maxUid)
        {
            return Context.UserRoles
                .Where(ur => ur.UserId >= minUid && ur.UserId <= maxUid)
                .ToListAsync();
        }

        public virtual Task<Dictionary<int, Role>> ListNamedRolesAsync()
        {
            return Context.Roles
                .Where(r => r.ShortName != null)
                .ToDictionaryAsync(r => r.Id);
        }

        public virtual Task<List<string>> ListSubscribedEmailsAsync()
        {
            var query = from u in Users
                        where u.EmailConfirmed && u.SubscribeNews
                        orderby u.Id ascending
                        select u.Email;
            return query.ToListAsync();
        }
    }
}
