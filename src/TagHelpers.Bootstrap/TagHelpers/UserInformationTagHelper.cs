using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Renders the user link.
    /// </summary>
    [HtmlTargetElement("user", Attributes = "uid", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("user", Attributes = "username", TagStructure = TagStructure.WithoutEndTag)]
    public class UserInformationTagHelper : XysTagHelper
    {
        private static readonly IReadOnlyDictionary<string, string> _emptyInfo = new Dictionary<string, string>();
        private readonly IUserInformationProvider _userInformationProvider;
        private IDictionary<string, string>? _attachInformation;

        /// <summary>
        /// The user ID
        /// </summary>
        [HtmlAttributeName("uid")]
        public int? UserId { get; set; }

        /// <summary>
        /// The user name
        /// </summary>
        [HtmlAttributeName("username")]
        public string? UserName { get; set; }

        /// <summary>
        /// The attach information
        /// </summary>
        [HtmlAttributeName("all-attach-data", DictionaryAttributePrefix = "attach-")]
        public IDictionary<string, string> AttachInformation
        {
            get => _attachInformation ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            set => _attachInformation = value ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The view context
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; } = null!;

        /// <summary>
        /// Constructs a <see cref="UserInformationTagHelper"/> for displaying user information.
        /// </summary>
        /// <param name="provider">The <see cref="IUserInformationProvider"/>.</param>
        public UserInformationTagHelper(IUserInformationProvider provider)
        {
            _userInformationProvider = provider;
        }

        /// <inheritdoc />
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            TagBuilder tag;

            if (UserId.HasValue)
            {
                tag = await _userInformationProvider.ProcessAsync(
                    UserId.Value,
                    UserName,
                    ((IReadOnlyDictionary<string, string>?)_attachInformation) ?? _emptyInfo,
                    ViewContext);
            }
            else if (UserName != null)
            {
                tag = await _userInformationProvider.ProcessAsync(
                    UserName,
                    ((IReadOnlyDictionary<string, string>?)_attachInformation) ?? _emptyInfo,
                    ViewContext);
            }
            else
            {
                throw new InvalidOperationException("Please specify at least ID or username.");
            }

            output.TagName = tag.TagName;
            output.TagMode = TagMode.StartTagAndEndTag;
            output.MergeAttributes(tag);
            output.Content.SetHtmlContent(tag.InnerHtml);
        }
    }
}
