using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    public class RemoveInertScpConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (!controller.ControllerType.IsSubclassOf(typeof(ViewControllerBase))
                || controller.Attributes.OfType<SupportStatusCodePageAttribute>().Any())
            {
                return;
            }

            for (int i = 0; i < controller.Actions.Count; i++)
            {
                var method = controller.Actions[i].ActionMethod;
                if (method.Name == nameof(ViewControllerBase.StatusCodePage)
                    && method.DeclaringType == typeof(ViewControllerBase))
                {
                    controller.Actions.RemoveAt(i--);
                }
            }
        }
    }
}
