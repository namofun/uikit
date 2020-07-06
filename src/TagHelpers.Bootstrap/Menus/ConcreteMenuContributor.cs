using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <inheritdoc cref="IMenuContributor" />
    internal class ConcreteMenuContributor : IMenuContributor, IMenuProvider
    {
        /// <inheritdoc />
        public Dictionary<string, IMenuEntryBuilderBase> Store { get; }

        /// <summary>
        /// Instantiate the <see cref="IMenuContributor"/>.
        /// </summary>
        public ConcreteMenuContributor()
        {
            Store = new Dictionary<string, IMenuEntryBuilderBase>();
        }

        /// <inheritdoc />
        public void Menu(string name, Action<IMenuBuilder> action)
        {
            IMenuBuilder menu2;
            if (!Store.TryGetValue(name, out var menu))
                Store.Add(name, menu2 = new ConcreteMenuBuilder(this));
            else if (menu is IMenuBuilder menu3)
                menu2 = menu3;
            else
                throw new InvalidOperationException(
                    $"\"{name}\" is not a menu.");
            action.Invoke(menu2);
        }

        /// <inheritdoc />
        public void Submenu(string name, Action<ISubmenuBuilder> action)
        {
            if (!Store.TryGetValue(name, out var menu) || !(menu is ISubmenuBuilder submenu))
                throw new InvalidOperationException(
                    $"\"{name}\" is not a pre-defined submenu. " +
                    $"Please add it in the corresponding menu first.");
            action.Invoke(submenu);
        }

        /// <inheritdoc />
        public IMenu Find(string name)
        {
            if (!Store.TryGetValue(name, out var menu) || !(menu is ConcreteMenuBuilder menu2))
                throw new InvalidOperationException(
                    $"\"{name}\" is not a pre-defined menu. " +
                    $"Please add it in the corresponding menu first.");
            menu2.Contribute();
            return menu2;
        }
    }
}
