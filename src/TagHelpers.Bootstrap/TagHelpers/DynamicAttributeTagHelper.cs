using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Dynamicly add attributes to tags.
    /// </summary>
    [HtmlTargetElement(Attributes = UseReadonlyKey)]
    [HtmlTargetElement(Attributes = UseCheckedKey)]
    [HtmlTargetElement(Attributes = UseSelectedKey)]
    public class DynamicAttributeTagHelper : XysTagHelper
    {
        private const string UseReadonlyKey = "bs-readonly-attr";
        private const string UseCheckedKey = "bs-checked-attr";
        private const string UseSelectedKey = "bs-selected-attr";

        /// <summary>
        /// Whether this input or textarea is readonly
        /// </summary>
        [HtmlAttributeName(UseReadonlyKey)]
        public bool UseReadonly { get; set; }

        /// <summary>
        /// Whether this radio or checkbox is checked
        /// </summary>
        [HtmlAttributeName(UseCheckedKey)]
        public bool UseChecked { get; set; }

        /// <summary>
        /// Whether this option is selected
        /// </summary>
        [HtmlAttributeName(UseSelectedKey)]
        public bool UseSelected { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if (UseReadonly) output.Attributes.Add(new TagHelperAttribute("readonly"));
            if (UseChecked) output.Attributes.Add(new TagHelperAttribute("checked"));
            if (UseSelected) output.Attributes.Add(new TagHelperAttribute("selected"));
        }
    }

    /// <summary>
    /// Dynamicly add attributes to tags.
    /// </summary>
    [HtmlTargetElement("div", Attributes = "[class='modal fade']," + UseStaticBackdropKey)]
    public class DynamicAttribute2TagHelper : XysTagHelper
    {
        private const string UseStaticBackdropKey = "bs-modal-static-backdrop";

        /// <summary>
        /// Whether to add data-backdrop
        /// </summary>
        [HtmlAttributeName(UseStaticBackdropKey)]
        public bool UseStaticBackdrop { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            if (UseStaticBackdrop)
            {
                output.Attributes.Add("data-backdrop", "static");
                output.Attributes.Add("data-keyboard", "false");
            }
        }
    }

    /// <summary>
    /// Dynamicly add attributes to tags.
    /// </summary>
    [HtmlTargetElement("form", Attributes = "[" + FormAjaxUploadKey + "='1']")]
    public class DynamicAttribute3TagHelper : XysTagHelper
    {
        private const string FormAjaxUploadKey = "bs-form-check-ajaxupload";

        /// <summary>
        /// The target for form ajax upload
        /// </summary>
        [HtmlAttributeName(FormAjaxUploadKey)]
        public string? FormAjaxUpload { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = null!;

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ViewContext.ViewData.ContainsKey("FormAjaxUpload"))
            {
                output.Attributes.RemoveAll("action");
                output.Attributes.Add("action", ViewContext.ViewData["FormAjaxUpload"]);
                output.Attributes.Add("enctype", "multipart/form-data");

                if (ViewContext.ViewData.ContainsKey("InAjax"))
                {
                    var handleKey = (string?)ViewContext.ViewData["HandleKey"];
                    var handleKey2 = (string?)ViewContext.ViewData["HandleKey2"];
                    output.Attributes.Add("onsubmit", $"ajaxpost(this,'{handleKey2}','{handleKey}');return false");
                }
            }
        }
    }
}
