using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    public class ControllerActionDescriptorWrapper
    {
        private ControllerActionDescriptor? _value;
        private readonly string _area, _controller, _action;

        public ControllerActionDescriptorWrapper(string area, string controller, string action)
        {
            _area = area;
            _controller = controller;
            _action = action;
        }

        public ControllerActionDescriptor GetValue(IServiceProvider services)
        {
            if (_value != null) return _value;

            var adcp = services.GetRequiredService<IActionDescriptorCollectionProvider>();
            var actions = adcp.ActionDescriptors.Items;
            var action = actions.OfType<ControllerActionDescriptor>()
                .Where(s => s.ControllerName.Equals(_controller, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.ActionName.Equals(_action, StringComparison.OrdinalIgnoreCase))
                .Where(s => s.RouteValues.TryGetValue("area", out var AreaName) && AreaName.Equals(_area, StringComparison.OrdinalIgnoreCase))
                .Single();

            return _value = action;
        }
    }
}
