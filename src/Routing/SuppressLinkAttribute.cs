using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Suppress the link generation and path matching for this action.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SuppressLinkAttribute : Attribute, IActionModelConvention
    {
        /// <inheritdoc />
        public void Apply(ActionModel action)
        {
            var selectors = action.Selectors;
            for (int i = 0; i < selectors.Count; i++)
            {
                var arm = selectors[i].AttributeRouteModel;
                arm.SuppressLinkGeneration = true;
                arm.SuppressPathMatching = true;
            }
        }
    }
}
