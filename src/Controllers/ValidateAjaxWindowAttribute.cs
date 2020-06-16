using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied validates the
    /// <c>handlekey</c> token. If the handle-key token is not available, the validation
    /// will fail and the action method will not execute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAjaxWindowAttribute : Attribute, IActionFilter
    {
        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Here is nothing to do.
        }

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is Controller2 controller))
                throw new InvalidOperationException();
            if (!controller.IsWindowAjax)
                context.Result = controller.BadRequest();
        }
    }
}
