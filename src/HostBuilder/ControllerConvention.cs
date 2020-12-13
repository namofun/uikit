using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Explicitly set the visibility of APIs.
    /// </summary>
    internal sealed class SubstrateControllerConvention : IControllerModelConvention
    {
        private readonly Dictionary<string, string> _decl = new Dictionary<string, string>();

        /// <summary>
        /// Declare the API when <see cref="IEndpointBuilder.MapApiDocument(string, string, string, string)"/> is called.
        /// </summary>
        /// <param name="assemblyName">The assembly name.</param>
        /// <param name="groupName">The group name.</param>
        public void Declare(string assemblyName, string groupName)
        {
            _decl.TryAdd(assemblyName, groupName);
        }

        /// <summary>
        /// Apply the rules to controller model.
        /// </summary>
        /// <param name="controller">The controller model.</param>
        public void Apply(ControllerModel controller)
        {
            bool isVisible = controller.ControllerType.IsSubclassOf(typeof(ApiControllerBase));
            if (!isVisible && !controller.ControllerType.IsSubclassOf(typeof(ViewControllerBase)))
                throw new ApplicationException("Controllers must inherit from ApiControllerBase or ViewControllerBase.");

            if (isVisible && _decl.TryGetValue(controller.ControllerType.Assembly.FullName!, out var groupName))
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
