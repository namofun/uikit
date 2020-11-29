using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <summary>
    /// The menu to render components.
    /// </summary>
    public interface IComponentMenu
    {
        /// <summary>
        /// Render the menus.
        /// </summary>
        /// <param name="helper">The view component helper.</param>
        /// <param name="model">The param model.</param>
        /// <returns>The task for rendering.</returns>
        Task<IHtmlContent> RenderAsync(IViewComponentHelper helper, object? model = null);
    }

    /// <summary>
    /// The menu builder for component extension point.
    /// </summary>
    public interface IComponentMenuBuilder
    {
        /// <summary>
        /// The final components
        /// </summary>
        IReadOnlyCollection<(int, Type)> Components { get; }

        /// <summary>
        /// Whether this builder is finalized
        /// </summary>
        bool Finalized { get; }

        /// <summary>
        /// Contribute the menus.
        /// </summary>
        void Contribute();

        /// <summary>
        /// Gets the name of this view component.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Returns whether this view component is OK.</returns>
        public static bool Validate(Type type)
        {
            if (!typeof(ViewComponent).IsAssignableFrom(type))
                return false;
            if (!type.IsClass || type.IsAbstract)
                return false;
            var invoke = type.GetMethod("Invoke", Type.EmptyTypes);
            var invokeAsync = type.GetMethod("InvokeAsync", Type.EmptyTypes);
            if ((invoke == null) == (invokeAsync == null))
                return false;
            if (invoke != null && invoke.ReturnType != typeof(IViewComponentResult))
                return false;
            if (invokeAsync != null && invokeAsync.ReturnType != typeof(Task<IViewComponentResult>))
                return false;
            return true;
        }

        /// <summary>
        /// Add a component to this menu extension point.
        /// </summary>
        /// <typeparam name="TComponent">The type of component.</typeparam>
        /// <param name="priority">The priority.</param>
        /// <returns>The builder to chain.</returns>
        public IComponentMenuBuilder HasComponent<TComponent>(int priority)
            where TComponent : ViewComponent
        {
            const string errorMsg = "This component does not satisfy the requirements.";
            if (Finalized || !Validate(typeof(TComponent)))
                throw new InvalidOperationException(errorMsg);
            ((ICollection<(int, Type)>)Components).Add((priority, typeof(TComponent)));
            return this;
        }
    }
}
