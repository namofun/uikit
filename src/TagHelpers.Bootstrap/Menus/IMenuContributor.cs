using Microsoft.AspNetCore.Mvc.Menus;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// The menu configuration contributor.
    /// </summary>
    public interface IMenuContributor
    {
        /// <summary>
        /// The stored menu entries.
        /// </summary>
        Dictionary<string, IMenuEntryBuilderBase> Store { get; }

        /// <summary>
        /// Configure the submenu.
        /// </summary>
        /// <param name="name">The menu identifier.</param>
        /// <param name="action">The menu builder.</param>
        void Submenu(string name, Action<ISubmenuBuilder> action);

        /// <summary>
        /// Configure the menu.
        /// </summary>
        /// <param name="name">The menu identifier.</param>
        /// <param name="action">The menu builder.</param>
        void Menu(string name, Action<IMenuBuilder> action);

        /// <summary>
        /// Configure the component list.
        /// </summary>
        /// <param name="name">The name identifier.</param>
        /// <returns>The component menu builder.</returns>
        IComponentMenuBuilder Component(string name);
    }

    /// <summary>
    /// The menu provider.
    /// </summary>
    public interface IMenuProvider
    {
        /// <summary>
        /// Gets the corresponding menu.
        /// </summary>
        /// <param name="name">The menu name.</param>
        /// <returns>The menu.</returns>
        IMenu Find(string name);

        /// <summary>
        /// Gets the component extensions for such menu.
        /// </summary>
        /// <param name="name">The name identifier.</param>
        /// <returns>The component menu.</returns>
        IComponentMenu Extend(string name);
    }
}
