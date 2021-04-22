using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
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
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <returns>A <see cref="Task"/> that on completion updates the output.</returns>
        Task ProcessAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output);

        /// <summary>
        /// Asynchronously produces the user information tag.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="attach">The attach informations.</param>
        /// <param name="actionContext">The current view context.</param>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <returns>A <see cref="Task"/> that on completion updates the output.</returns>
        Task ProcessAsync(
            string userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output);
    }

    /// <summary>
    /// The default null implemention to service the user information tag helper.
    /// </summary>
    public class NullUserInformationProvider : IUserInformationProvider
    {
        /// <inheritdoc />
        public Task ProcessAsync(
            int userId,
            string? userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", "#");
            output.Content.SetContent(userName ?? ("User u" + userId));
            output.TagMode = TagMode.StartTagAndEndTag;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ProcessAsync(
            string userName,
            IReadOnlyDictionary<string, string> attach,
            ViewContext actionContext,
            TagHelperContext context,
            TagHelperOutput output)
        {
            output.TagName = "a";
            output.Attributes.SetAttribute("href", "#");
            output.Content.SetContent(userName);
            output.TagMode = TagMode.StartTagAndEndTag;
            return Task.CompletedTask;
        }
    }
}
