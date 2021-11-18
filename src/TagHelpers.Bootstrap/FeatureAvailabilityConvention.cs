using System;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    public class FeatureAvailabilityConvention : IApplicationModelConvention
    {
        private readonly Type[] _controllerTypes;
        private readonly bool _available;

        public void Apply(ApplicationModel application)
        {
            if (_available) return;

            application.Controllers
                .Where(c => _controllerTypes.Any(a => a == c.ControllerType.AsType()))
                .ToList()
                .ForEach(cm => application.Controllers.Remove(cm));
        }

        public FeatureAvailabilityConvention(
            bool available,
            params Type[] controllerTypes)
        {
            _controllerTypes = controllerTypes;
            _available = available;
        }
    }
}
