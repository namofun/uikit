using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// The menu entry base.
    /// </summary>
    public interface IMenuEntryBase
    {
        /// <summary>
        /// The id of menu entry
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The title of menu entry
        /// </summary>
        string Title { get; }

        /// <summary>
        /// The icon of menu entry
        /// </summary>
        string Icon { get; }

        /// <summary>
        /// The priority of menu entry
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// The badges to show.
        /// </summary>
        IEnumerable<(string, BootstrapColor)> Badges { get; }

        /// <summary>
        /// Get the destination link of menu entry.
        /// </summary>
        /// <param name="urlHelper">The <see cref="IUrlHelper"/> to generate links.</param>
        /// <returns>The generated link.</returns>
        string GetLink(IUrlHelper urlHelper);

        /// <summary>
        /// Check whether this tag is active for such <see cref="ActionContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <returns>Whether this tag is active.</returns>
        bool IsActive(ViewContext actionContext);

        /// <summary>
        /// Check whether this tag is active for such <see cref="ActionContext"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <returns>Whether this tag is active.</returns>
        public string Active(ViewContext viewContext) => IsActive(viewContext) ? "active" : "";

        /// <summary>
        /// Check whether to show this menu entry.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>The decision.</returns>
        bool Satisfy(HttpContext httpContext);
    }

    /// <summary>
    /// The menu entry.
    /// </summary>
    public interface IMenuEntry : IMenuEntryBase
    {
    }

    /// <summary>
    /// The submenu.
    /// </summary>
    public interface ISubmenu : IMenuEntryBase, IReadOnlyList<IMenuEntry>
    {
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// The menu.
    /// </summary>
    public interface IMenu : IReadOnlyList<IMenuEntryBase>
    {
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
