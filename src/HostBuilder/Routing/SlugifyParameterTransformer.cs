using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Routing
{
    /// <summary>
    /// The <see cref="IOutboundParameterTransformer"/> to make inbound and outbound
    /// route token from <c>PascalNameMethod</c> to <c>pascal-name-method</c>.
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        /// <inheritdoc />
        public string? TransformOutbound(object? value)
        {
            return value == null
                ? null
                : Regex.Replace(value.ToString()!, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}
