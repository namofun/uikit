using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <summary>
    /// The main menu builder.
    /// </summary>
    public interface IMenuBuilder : IMenuEntryBuilderBase
    {
        /// <summary>
        /// Add a submenu to this menu.
        /// </summary>
        /// <param name="priority">The submenu priority.</param>
        /// <param name="configure">The submenu configure.</param>
        void HasSubmenu(int priority, Action<ISubmenuBuilder> configure);

        /// <summary>
        /// Add a submenu to this menu.
        /// </summary>
        /// <param name="name">The submenu identifier.</param>
        /// <param name="priority">The submenu priority.</param>
        /// <param name="configure">The submenu configure.</param>
        internal void HasSubmenu(string name, int priority, Action<ISubmenuBuilder> configure);

        /// <summary>
        /// Add a menu entry to this menu.
        /// </summary>
        /// <param name="priority">The order of this menu.</param>
        /// <returns>The <see cref="IMenuEntryBuilder"/>.</returns>
        IMenuEntryBuilder HasEntry(int priority);

        /// <summary>
        /// Add a menu entry to this menu.
        /// </summary>
        /// <param name="priority">The order of this menu.</param>
        /// <param name="configure">The <see cref="IMenuEntryBuilder"/>.</param>
        public void HasEntry(int priority, Action<IMenuEntryBuilder> configure)
        {
            configure.Invoke(HasEntry(priority));
        }
    }

    /// <summary>
    /// The base class to configure the menu entries and provide functions to finalize the model.
    /// </summary>
    public interface IMenuEntryBuilderBase
    {
        /// <summary>
        /// The menu contributor
        /// </summary>
        IMenuContributor Contributor { get; }

        /// <summary>
        /// Provide metadata for this menu entry.
        /// </summary>
        Dictionary<string, object> Metadata { get; }

        /// <summary>
        /// Provide requirements for this menu entry.
        /// </summary>
        List<Expression<Func<ViewContext, bool>>> Requirements { get; }

        /// <summary>
        /// Provide activities for this menu entry.
        /// </summary>
        List<Expression<Func<ViewContext, bool>>> Activities { get; }

        /// <summary>
        /// Whether this model is finalized.
        /// </summary>
        bool Finalized { get; }

        /// <summary>
        /// Finalize the configurations.
        /// </summary>
        void Contribute();
    }

    /// <summary>
    /// The builder to configure this menu entry.
    /// </summary>
    public interface IMenuEntryBuilder : IMenuEntryBuilderBase
    {
    }

    /// <summary>
    /// The builder for submenu.
    /// </summary>
    public interface ISubmenuBuilder : IMenuEntryBuilderBase
    {
        /// <summary>
        /// Add a menu entry to this menu.
        /// </summary>
        /// <param name="priority">The order of this menu.</param>
        /// <returns>The <see cref="IMenuEntryBuilder"/>.</returns>
        IMenuEntryBuilder HasEntry(int priority);

        /// <summary>
        /// Add a menu entry to this menu.
        /// </summary>
        /// <param name="priority">The order of this menu.</param>
        /// <param name="configure">The <see cref="IMenuEntryBuilder"/>.</param>
        public void HasEntry(int priority, Action<IMenuEntryBuilder> configure)
        {
            configure.Invoke(HasEntry(priority));
        }
    }
}
