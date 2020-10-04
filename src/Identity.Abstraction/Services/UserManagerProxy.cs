using Microsoft.AspNetCore.Identity;
using SatelliteSite.IdentityModule.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SatelliteSite.IdentityModule.Services
{
    internal class UserManagerProxy<TUser, TRole, TUserManager> :
        IUserManager
        where TUser : User, new()
        where TRole : Role, new()
        where TUserManager : UserManagerBase<TUser, TRole>
    {
        private readonly TUserManager _origin;

        public UserManagerProxy(TUserManager userManager)
        {
            _origin = userManager;
        }

        public Task<IdentityResult> AddToRoleAsync(IUser user, string role) => _origin.AddToRoleAsync((TUser)user, role);
        public Task<IdentityResult> AddToRolesAsync(IUser user, IEnumerable<string> roles) => _origin.AddToRolesAsync((TUser)user, roles);
        public Task<bool> IsInRoleAsync(IUser user, string role) => _origin.IsInRoleAsync((TUser)user, role);
        public Task<IdentityResult> RemoveFromRoleAsync(IUser user, string role) => _origin.RemoveFromRoleAsync((TUser)user, role);
        public Task<IdentityResult> RemoveFromRolesAsync(IUser user, IEnumerable<string> roles) => _origin.RemoveFromRolesAsync((TUser)user, roles);
        public Task<IList<string>> GetRolesAsync(IUser user) => _origin.GetRolesAsync((TUser)user);
        public Task<IReadOnlyList<IRole>> ListRolesAsync(IUser user) => _origin.ListRolesAsync((TUser)user);
        public Task<ILookup<int, int>> ListUserRolesAsync(int minUid, int maxUid) => _origin.ListUserRolesAsync(minUid, maxUid);
        public Task<IReadOnlyDictionary<int, IRole>> ListNamedRolesAsync() => _origin.ListNamedRolesAsync();
        public async Task<IReadOnlyList<IUser>> GetUsersInRoleAsync(string roleName) => (List<TUser>)await _origin.GetUsersInRoleAsync(roleName);

        public Task<IdentityResult> DeleteAsync(IUser user) => _origin.DeleteAsync((TUser)user);
        public async Task<IUser> FindByNameAsync(string userName) => await _origin.FindByNameAsync(userName);
        public async Task<IUser> FindByEmailAsync(string email) => await _origin.FindByEmailAsync(email);
        public async Task<IUser> FindByIdAsync(int userId) => await _origin.FindByIdAsync($"{userId}");
        public Task<IdentityResult> UpdateAsync(IUser user) => _origin.UpdateAsync((TUser)user);
        public Task<IdentityResult> CreateAsync(IUser user) => _origin.CreateAsync((TUser)user);
        public Task<IPagedList<IUser>> ListUsersAsync(int page, int pageCount) => _origin.ListUsersAsync(page, pageCount);

        public string GetUserName(ClaimsPrincipal principal) => _origin.GetUserName(principal);
        public string GetNickName(ClaimsPrincipal principal) => _origin.GetNickName(principal);
        public async Task<IUser> GetUserAsync(ClaimsPrincipal principal) => await _origin.GetUserAsync(principal);
        public int? GetUserId(ClaimsPrincipal principal)
        {
            string result = _origin.GetUserId(principal);
            if (result == null) return null;
            return int.Parse(result);
        }

        public Task<IdentityResult> CreateAsync(IUser user, string password) => _origin.CreateAsync((TUser)user, password);
        public Task<bool> HasPasswordAsync(IUser user) => _origin.HasPasswordAsync((TUser)user);
        public Task<IdentityResult> AddPasswordAsync(IUser user, string password) => _origin.AddPasswordAsync((TUser)user, password);
        public Task<IdentityResult> ChangePasswordAsync(IUser user, string currentPassword, string newPassword) => _origin.ChangePasswordAsync((TUser)user, currentPassword, newPassword);
        public Task<string> GeneratePasswordResetTokenAsync(IUser user) => _origin.GeneratePasswordResetTokenAsync((TUser)user);
        public Task<IdentityResult> ResetPasswordAsync(IUser user, string token, string newPassword) => _origin.ResetPasswordAsync((TUser)user, token, newPassword);

        public Task<bool> IsEmailConfirmedAsync(IUser user) => _origin.IsEmailConfirmedAsync((TUser)user);
        public Task<string> GenerateEmailConfirmationTokenAsync(IUser user) => _origin.GenerateEmailConfirmationTokenAsync((TUser)user);
        public Task<IdentityResult> ConfirmEmailAsync(IUser user, string token) => _origin.ConfirmEmailAsync((TUser)user, token);
        public Task<IdentityResult> SetEmailAsync(IUser user, string email) => _origin.SetEmailAsync((TUser)user, email);
        public Task<IReadOnlyList<string>> ListSubscribedEmailsAsync() => _origin.ListSubscribedEmailsAsync();

        public IUser CreateEmpty(string username) => new TUser() { UserName = username };

    }
}
