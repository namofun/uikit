using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// The base controller class for APIs.
    /// </summary>
    public abstract class ApiControllerBase : ControllerBase, IAsyncActionFilter
    {
        /// <summary>
        /// Called after the action method is invoked.
        /// </summary>
        /// <param name="context">The action executed context.</param>
        /// <returns>The <see cref="Task"/> for filter to do after action executed.</returns>
        [NonAction]
        public virtual Task OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value == null)
                    objectResult.StatusCode = 404;
            }

            return Task.CompletedTask;
        }


        /// <summary>
        /// Called before the action method is invoked.
        /// </summary>
        /// <param name="context">The action executing context.</param>
        /// <returns>The <see cref="Task"/> for filter to do before action executing.</returns>
        [NonAction]
        public virtual Task OnActionExecuting(ActionExecutingContext context)
        {
            return Task.CompletedTask;
        }


        /// <inheritdoc />
        [NonAction]
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await OnActionExecuting(context);

            if (context.Result == null)
            {
                var actionResult = await next();
                await OnActionExecuted(actionResult);
            }
        }
    }
}
