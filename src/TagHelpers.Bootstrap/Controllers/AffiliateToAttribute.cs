using System;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Marks the controller class is affiliated to some module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
    public sealed class AffiliateToAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of required modules.
        /// </summary>
        public Type[] ModuleTypes { get; }

        /// <summary>
        /// Gets the type of belonging module.
        /// </summary>
        public Type BelongingModuleType { get; }

        /// <summary>
        /// Gets the type of connector.
        /// </summary>
        public Type ConnectorType { get; }

        /// <summary>
        /// Marks the assembly is affiliated to the module with the connector.
        /// </summary>
        /// <param name="connectorType">The connector type.</param>
        /// <param name="belongModuleType">The belonging module type.</param>
        /// <param name="otherModuleTypes">The other module types. Only the others are all loaded will this connector be used.</param>
        public AffiliateToAttribute(Type connectorType, Type belongModuleType, params Type[] otherModuleTypes)
        {
            ConnectorType = connectorType;
            ModuleTypes = (otherModuleTypes ?? Type.EmptyTypes).Append(belongModuleType).ToArray();
            BelongingModuleType = belongModuleType;
        }
    }
}
