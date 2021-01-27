using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// Provides an abstraction for a store which manages user accounts with listing features.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    /// <typeparam name="TRole">The type encapsulating a role.</typeparam>
    public interface IUserListStore<TUser, TRole> : IUserStore<TUser>
        where TUser : class
        where TRole : class
    {
        /// <summary>
        /// Gets the paged list of users.
        /// </summary>
        /// <param name="page">The page id.</param>
        /// <param name="pageCount">The count per page.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The task for fetching user list.</returns>
        Task<IPagedList<TUser>> ListAsync(int page, int pageCount, CancellationToken cancellationToken);

        /// <summary>
        /// Lists roles of users.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The task for fetching role list.</returns>
        Task<List<TRole>> ListRolesAsync(TUser user, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the list of named roles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The task for fetching role dictionary.</returns>
        Task<List<TRole>> ListNamedRolesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the paged list of user roles.
        /// </summary>
        /// <param name="minUserId">The minimum user ID.</param>
        /// <param name="maxUserId">The maximum user ID.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The task for fetching user role list.</returns>
        Task<ILookup<int, int>> ListUserRolesAsync(int minUserId, int maxUserId, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the mail list of users with subscription.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The task for fetching user list.</returns>
        Task<List<string>> ListSubscribedEmailsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets a flag indicating whether the specified <paramref name="normalizedRoleName"/> exists.
        /// </summary>
        /// <param name="normalizedRoleName">The normalized role name whose existence should be checked.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing true if the role name exists, otherwise false.</returns>
        Task<bool> ExistRoleAsync(string normalizedRoleName, CancellationToken cancellationToken);

        /// <summary>
        /// Locks out all users in the <paramref name="userIds"/>.
        /// </summary>
        /// <param name="userIds">The user IDs whom should be locked out.</param>
        /// <param name="newSecurityStamp">The new security stamp.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, returning the count of affected users.</returns>
        Task<int> LockOutUsersAsync(IEnumerable<int> userIds, string newSecurityStamp, CancellationToken cancellationToken);

        /// <summary>
        /// Finds the user names in the <paramref name="userIds"/>.
        /// </summary>
        /// <param name="userIds">The user IDs.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The dictionary of user name.</returns>
        Task<Dictionary<int, string>> ListUserNamesAsync(IEnumerable<int> userIds, CancellationToken cancellationToken);

        /// <summary>
        /// Finds the role claims for <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operations.</param>
        /// <returns>The enumerable of role claims.</returns>
        Task<List<Claim>> ListUserRoleClaimsAsync(TUser user, CancellationToken cancellationToken);
    }
}
