using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Dynamicly add attributes to tags.
    /// </summary>
    [HtmlTargetElement("div", Attributes = "[class=modal-dialog]," + UseStaticBackdropKey)]
    [HtmlTargetElement(Attributes = UseReadonlyKey)]
    [HtmlTargetElement(Attributes = UseCheckedKey)]
    [HtmlTargetElement(Attributes = UseSelectedKey)]
    public class DynamicAttributeTagHelper : XysTagHelper
    {
        private const string UseReadonlyKey = "bs-readonly-attr";
        private const string UseCheckedKey = "bs-checked-attr";
        private const string UseSelectedKey = "bs-selected-attr";
        private const string UseStaticBackdropKey = "bs-modal-static-backdrop";

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

        /// <summary>
        /// Whether to add use-backdrop
        /// </summary>
        [HtmlAttributeName(UseStaticBackdropKey)]
        public bool UseStaticBackdrop { get; set; }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (UseReadonly)
            {
                output.Attributes.Add(new TagHelperAttribute("readonly"));
            }

            if (UseChecked)
            {
                output.Attributes.Add(new TagHelperAttribute("checked"));
            }

            if (UseSelected)
            {
                output.Attributes.Add(new TagHelperAttribute("selected"));
            }

            if (UseStaticBackdrop)
            {
                output.Attributes.Add("data-backdrop", "static");
                output.Attributes.Add("data-keyboard", "false");
            }
        }
    }
}
