using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    /// <summary>
    /// Explicitly set the visibility of APIs.
    /// </summary>
    internal class ApiExplorerVisibilityAttribute : Attribute, IControllerModelConvention
    {
        /// <summary>
        /// The declared assemblies and its module
        /// </summary>
        public static Dictionary<string, string> DeclaredAssemblyModule { get; }
            = new Dictionary<string, string>();

        /// <summary>
        /// Whether this ActionDescriptor is visible in ApiExplorer
        /// </summary>
        public bool IsVisible { get; }

        /// <summary>
        /// Set the visibility of API.
        /// </summary>
        /// <param name="isVisible">Whether this ActionDescriptor is visible in ApiExplorer</param>
        public ApiExplorerVisibilityAttribute(bool isVisible)
        {
            IsVisible = isVisible;
        }

        /// <summary>
        /// Apply the rules to controller model.
        /// </summary>
        /// <param name="controller">The controller model.</param>
        public void Apply(ControllerModel controller)
        {
            if (IsVisible && DeclaredAssemblyModule.TryGetValue(controller.ControllerType.Assembly.FullName!, out var groupName))
            {
                controller.ApiExplorer.GroupName = groupName;
                controller.ApiExplorer.IsVisible = true;
            }
            else
            {
                controller.ApiExplorer.IsVisible = false;
            }
        }
    }
}
