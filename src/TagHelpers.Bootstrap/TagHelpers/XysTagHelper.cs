using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Razor.TagHelpers
{
    /// <inheritdoc cref="TagHelper" />
    public abstract class XysTagHelper : ITagHelper
    {
        /// <inheritdoc />
        public virtual int Order => 0;

        /// <inheritdoc />
        public virtual void Init(TagHelperContext context)
        {
            
        }

        /// <inheritdoc cref="TagHelper.Process(TagHelperContext, TagHelperOutput)" />
        public virtual void Process(TagHelperContext context, TagHelperOutput output)
        {
        }

        /// <inheritdoc cref="ITagHelperComponent.ProcessAsync(TagHelperContext, TagHelperOutput)" />
        public virtual Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            Process(context, output);
            return Task.CompletedTask;
        }

        Task ITagHelperComponent.ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (DisplayWhenTagHelper.Check(context))
                return Task.CompletedTask;
            return ProcessAsync(context, output);
        }
    }
}
