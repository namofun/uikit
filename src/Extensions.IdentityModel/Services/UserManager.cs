using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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
    /// <inheritdoc cref="UserManager{TUser}" />
    public class UserManager : UserManager<User>
    {
        /// <summary>
        /// Construct a new instance of <see cref="UserManager"/>.
        /// </summary>
        public UserManager(
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
            var contextProperty = store.GetType().GetProperty(nameof(Context));
            if (contextProperty == null)
                throw new InvalidOperationException("This user manager should be rewritten.");
            Context = (IdentityDbContext<User, Role, int>)contextProperty.GetValue(store);
        }

        /// <summary>
        /// Gets the database context for this store.
        /// </summary>
        private IdentityDbContext<User, Role, int> Context { get; }

        /// <inheritdoc />
        public override bool SupportsUserTwoFactor => false;

        /// <inheritdoc />
        public override bool SupportsUserTwoFactorRecoveryCodes => false;

        /// <inheritdoc />
        public override string GetUserId(ClaimsPrincipal principal)
        {
            // Be careful of the old logins
            return base.GetUserId(principal)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        /// <summary>
        /// Get the nick name of a <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <param name="claim">The <see cref="ClaimsPrincipal"/>.</param>
        /// <returns>The nick name or default user name.</returns>
        public string GetNickName(ClaimsPrincipal claim)
        {
            var nickName = claim.FindFirstValue("nickname");
            if (string.IsNullOrEmpty(nickName)) nickName = GetUserName(claim);
            return nickName;
        }

        /// <summary>
        /// Mark the user's information as slide expiration so that the user claims will be re-generated.
        /// </summary>
        /// <param name="user">The user entity.</param>
        /// <returns>The operation result.</returns>
        public Task<IdentityResult> SlideExpirationAsync(User user)
        {
            CookieAuthenticationValidator._cache.Set(
                key: "SlideExpiration: " + user.NormalizedUserName,
                value: DateTimeOffset.UtcNow,
                absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(20));
            return Task.FromResult(IdentityResult.Success);
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            var result = await base.AddToRoleAsync(user, role);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> AddToRolesAsync(User user, IEnumerable<string> roles)
        {
            var result = await base.AddToRolesAsync(user, roles);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> RemoveFromRoleAsync(User user, string role)
        {
            var result = await base.RemoveFromRoleAsync(user, role);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> RemoveFromRolesAsync(User user, IEnumerable<string> roles)
        {
            var result = await base.RemoveFromRolesAsync(user, roles);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <inheritdoc />
        public override async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            var result = await base.ConfirmEmailAsync(user, token);
            if (result.Succeeded) await SlideExpirationAsync(user);
            return result;
        }

        /// <summary>
        /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The user ID to search for.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the user matching the specified <paramref name="userId"/> if it exists.</returns>
        public virtual Task<User> FindByIdAsync(int userId)
        {
            return FindByIdAsync($"{userId}");
        }

        /// <summary>
        /// List roles of users.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>The task for fetching role list.</returns>
        public virtual Task<List<Role>> ListRolesAsync(User user)
        {
            var query = from userRole in Context.UserRoles
                        join role in Context.Roles on userRole.RoleId equals role.Id
                        where userRole.UserId.Equals(user.Id)
                        select role;
            return query.ToListAsync();
        }

        /// <summary>
        /// Gets the paged list of users.
        /// </summary>
        /// <param name="page">The page id.</param>
        /// <param name="pageCount">The count per page.</param>
        /// <returns>The task for fetching user list.</returns>
        public virtual Task<PagedViewList<User>> ListUsersAsync(int page, int pageCount)
        {
            return Users.OrderBy(u => u.Id).ToPagedListAsync(page, pageCount);
        }

        /// <summary>
        /// Gets the paged list of user roles.
        /// </summary>
        /// <param name="minUid">The minimum user ID.</param>
        /// <param name="maxUid">The maximum user ID.</param>
        /// <returns>The task for fetching user role list.</returns>
        public virtual async Task<ILookup<int, int>> ListUserRolesAsync(int minUid, int maxUid)
        {
            var lst = await Context.UserRoles
                .Where(ur => ur.UserId >= minUid && ur.UserId <= maxUid)
                .ToListAsync();
            return lst.ToLookup(a => a.UserId, a => a.RoleId);
        }

        /// <summary>
        /// Gets the dictionary of named roles.
        /// </summary>
        /// <returns>The task for fetching role dictionary.</returns>
        public virtual Task<Dictionary<int, Role>> ListNamedRolesAsync()
        {
            return Context.Roles.Where(r => r.ShortName != null).ToDictionaryAsync(r => r.Id);
        }

        /// <summary>
        /// Gets the mail list of users with subscription.
        /// </summary>
        /// <returns>The task for fetching user list.</returns>
        public virtual Task<List<string>> ListSubscribedEmailsAsync()
        {
            return Users
                .Where(u => u.EmailConfirmed && u.SubscribeNews)
                .OrderBy(u => u.Id)
                .Select(u => u.Email)
                .ToListAsync();
        }
    }
}
