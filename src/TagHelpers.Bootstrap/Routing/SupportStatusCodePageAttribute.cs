using System;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// Marks the controller that the status code page will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class SupportStatusCodePageAttribute : Attribute
    {
    }
}
