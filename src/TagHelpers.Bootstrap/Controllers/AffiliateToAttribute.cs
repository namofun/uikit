using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Marks the controller class is affiliated to some module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class AffiliateToAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of module affiliated to.
        /// </summary>
        public Type ModuleType { get; }

        /// <summary>
        /// Marks the controller is affiliated to the module.
        /// </summary>
        /// <param name="type">The module type.</param>
        public AffiliateToAttribute(Type type)
        {
            ModuleType = type;
        }
    }
}
