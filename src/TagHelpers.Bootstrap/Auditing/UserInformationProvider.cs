using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Identity
{
    /// <summary>
    /// The provider to service the user information tag helper.
    /// </summary>
    public interface IUserInformationProvider
    {
        /// <summary>
        /// Asynchronously produces the user information tag.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="userName">The user name, if specified.</param>
        /// <param name="attach">The attach informations.</param>
        /// <param name="actionContext">The current view context.</param>
        /// <returns>A <see cref="Task{T}"/> that contains the final render tag.</returns>
        ValueTask<TagBuilder> ProcessAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string>? attach,
            ViewContext actionContext);

        /// <summary>
        /// Asynchronously produces the user information tag.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="attach">The attach informations.</param>
        /// <param name="actionContext">The current view context.</param>
        /// <returns>A <see cref="Task{T}"/> that contains the final render tag.</returns>
        ValueTask<TagBuilder> ProcessAsync(
            string userName,
            IReadOnlyDictionary<string, string>? attach,
            ViewContext actionContext);
    }

    /// <summary>
    /// The default null implemention to service the user information tag helper.
    /// </summary>
    public class NullUserInformationProvider : IUserInformationProvider
    {
        /// <inheritdoc />
        public ValueTask<TagBuilder> ProcessAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string>? attach,
            ViewContext actionContext)
        {
            var tag = new TagBuilder("a");
            tag.MergeAttribute("href", "#");
            tag.InnerHtml.Append(userName ?? ("User u" + userId));
            return new ValueTask<TagBuilder>(tag);
        }

        /// <inheritdoc />
        public ValueTask<TagBuilder> ProcessAsync(
            string userName,
            IReadOnlyDictionary<string, string>? attach,
            ViewContext actionContext)
        {
            var tag = new TagBuilder("a");
            tag.MergeAttribute("href", "#");
            tag.InnerHtml.Append(userName);
            return new ValueTask<TagBuilder>(tag);
        }
    }
}

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// User information extension methods.
    /// </summary>
    public static class UserInformationExtensions
    {
        /// <summary>
        /// Asynchronously produces the user information tag.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="userName">The user name, if specified.</param>
        /// <param name="attach">The attach informations.</param>
        /// <param name="viewContext">The current view context.</param>
        /// <returns>A <see cref="Task{T}"/> that contains the final render tag.</returns>
        public static ValueTask<TagBuilder> User(
            this ViewContext viewContext,
            int userId,
            string? userName = null,
            IReadOnlyDictionary<string, string>? attach = null)
        {
            var provider = viewContext.HttpContext.RequestServices
                .GetRequiredService<Identity.IUserInformationProvider>();

            return provider.ProcessAsync(userId, userName, attach, viewContext);
        }

        /// <summary>
        /// Asynchronously produces the user information tag.
        /// </summary>
        /// <param name="userName">The user name, if specified.</param>
        /// <param name="attach">The attach informations.</param>
        /// <param name="viewContext">The current view context.</param>
        /// <returns>A <see cref="Task{T}"/> that contains the final render tag.</returns>
        public static ValueTask<TagBuilder> User(
            this ViewContext viewContext,
            string userName,
            IReadOnlyDictionary<string, string>? attach = null)
        {
            var provider = viewContext.HttpContext.RequestServices
                .GetRequiredService<Identity.IUserInformationProvider>();

            return provider.ProcessAsync(userName, attach, viewContext);
        }
    }
}
