using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// When <c>ViewData.Contains(asp-viewdata-key)</c> or <c>User.IsInRoles(asp-in-roles)</c> is not satisfied, suppress the tag output.
    /// </summary>
    [HtmlTargetElement(Attributes = ViewDataKey)]
    [HtmlTargetElement(Attributes = InRolesKey)]
    [HtmlTargetElement(Attributes = MeetPolicyKey)]
    [HtmlTargetElement(Attributes = ElseViewDataKey)]
    [HtmlTargetElement(Attributes = ElseInRolesKey)]
    [HtmlTargetElement(Attributes = ElseMeetPolicyKey)]
    [HtmlTargetElement(Attributes = ConditionKey)]
    public class DisplayWhenTagHelper : TagHelper
    {
        private const string ViewDataKey = "asp-viewdata-key";
        private const string InRolesKey = "asp-in-roles";
        private const string MeetPolicyKey = "asp-meet-policy";
        private const string ElseViewDataKey = "asp-no-viewdata-key";
        private const string ElseInRolesKey = "asp-not-in-roles";
        private const string ElseMeetPolicyKey = "asp-not-meet-policy";
        private const string ConditionKey = "asp-show-if";

        public override int Order => -10000;

#pragma warning disable CS8618
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
#pragma warning restore CS8618

        /// <summary>
        /// The required ViewData keys
        /// </summary>
        [HtmlAttributeName(ElseViewDataKey)]
        public string? ElseKey { get; set; }

        /// <summary>
        /// The required user roles
        /// </summary>
        [HtmlAttributeName(ElseInRolesKey)]
        public string? ElseRoles { get; set; }

        /// <summary>
        /// The required user policy
        /// </summary>
        [HtmlAttributeName(ElseMeetPolicyKey)]
        public string? ElsePolicy { get; set; }

        /// <summary>
        /// The required ViewData keys
        /// </summary>
        [HtmlAttributeName(ViewDataKey)]
        public string? Key { get; set; }

        /// <summary>
        /// The required user roles
        /// </summary>
        [HtmlAttributeName(InRolesKey)]
        public string? Roles { get; set; }

        /// <summary>
        /// The required user policy
        /// </summary>
        [HtmlAttributeName(MeetPolicyKey)]
        public string? Policy { get; set; }

        /// <summary>
        /// The display requirement
        /// </summary>
        [HtmlAttributeName(ConditionKey)]
        public bool ShowIf { get; set; } = true;

        /// <summary>
        /// Is the output suppressed
        /// </summary>
        [HtmlAttributeNotBound]
        private bool Suppressed { get; set; }

        public override void Init(TagHelperContext context)
        {
            base.Init(context);
            context.Items.Add("DisplayWhenTagHelper_" + context.UniqueId, this);
        }

        public static bool Check(TagHelperContext context)
        {
            var lst = context.Items.Values.OfType<DisplayWhenTagHelper>();
            return lst.Aggregate(false, (fst, tgh) => fst || tgh.Suppressed);
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            bool suppress = !ShowIf;
            var user = ViewContext.HttpContext.User;
            var viewData = ViewContext.ViewData;

            if (Key != null && !suppress)
            {
                suppress = !viewData.ContainsKey(Key);
            }

            if (Roles != null && !suppress)
            {
                suppress = !user.IsInRoles(Roles);
            }

            if (ElseKey != null && !suppress)
            {
                suppress = viewData.ContainsKey(ElseKey);
            }

            if (ElseRoles != null && !suppress)
            {
                suppress = user.IsInRoles(ElseRoles);
            }

            if (Policy != null && !suppress)
            {
                var handler = ViewContext.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var result = await handler.AuthorizeAsync(user, Policy);
                suppress = !result.Succeeded;
            }

            if (ElsePolicy != null && !suppress)
            {
                var handler = ViewContext.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
                var result = await handler.AuthorizeAsync(user, ElsePolicy);
                suppress = result.Succeeded;
            }

            if (suppress)
            {
                Suppressed = suppress;
                output.SuppressOutput();
            }
        }
    }

    /// <summary>
    /// Razor tag block without wrapping in output but in code.
    /// </summary>
    [HtmlTargetElement("razor")]
    public class EmptyWrapperTagHelper : XysTagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            output.TagName = null;
        }
    }
}
