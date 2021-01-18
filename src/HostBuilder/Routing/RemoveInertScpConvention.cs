using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.Routing
{
    public class RemoveInertScpConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.Attributes.OfType<SupportStatusCodePageAttribute>().Any())
            {
                return;
            }

            for (int i = 0; i < controller.Actions.Count; i++)
            {
                if (controller.Actions[i].ActionMethod.Name == "StatusCodePage")
                {
                    controller.Actions.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
