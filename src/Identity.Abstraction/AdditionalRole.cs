#nullable enable
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SatelliteSite.IdentityModule.Models
{
    /// <summary>
    /// The definition of addition.
    /// </summary>
    public interface IAdditionalRole
    {
        /// <summary>
        /// The category of addition
        /// </summary>
        string Category { get; }

        /// <summary>
        /// The title of addition
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The display text of addition
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets a URL for this resource.
        /// </summary>
        /// <param name="urlHelper">The IUrlHelper.</param>
        /// <returns>The url of this resource.</returns>
        string? GetUrl(object urlHelper);
    }

    /// <summary>
    /// The notification for additional roles to show on dashboard page.
    /// </summary>
    public class UserDetailModel : INotification
    {
        private readonly List<IAdditionalRole> _list;

        /// <summary>
        /// The user entity
        /// </summary>
        public IUser User { get; }

        /// <summary>
        /// The role entities
        /// </summary>
        public IReadOnlyList<IRole> Roles { get; }

        /// <summary>
        /// The additions
        /// </summary>
        public IReadOnlyList<IAdditionalRole> Additions => _list;

        /// <inheritdoc cref="List{T}.Add(T)" />
        public void AddMore(IAdditionalRole item) => _list.Add(item);

        /// <inheritdoc cref="List{T}.AddRange(IEnumerable{T})" />
        public void AddMore(IEnumerable<IAdditionalRole> collection) => _list.AddRange(collection);

        /// <summary>
        /// Creates a <see cref="UserDetailModel"/>.
        /// </summary>
        public UserDetailModel(IUser user, IReadOnlyList<IRole> roles)
        {
            User = user;
            Roles = roles;
            _list = new List<IAdditionalRole>();
        }
    }
}
