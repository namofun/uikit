﻿using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <inheritdoc cref="IMenuContributor" />
    internal class ConcreteMenuContributor : IMenuContributor, IMenuProvider
    {
        /// <inheritdoc />
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        public Dictionary<string, IMenuEntryBuilderBase> Store { get; }

        /// <inheritdoc />
        public Dictionary<string, IComponentMenuBuilder> Components { get; }

        /// <summary>
        /// Instantiate the <see cref="IMenuContributor"/>.
        /// </summary>
        public ConcreteMenuContributor(IServiceProvider serviceProvider)
        {
            Store = new Dictionary<string, IMenuEntryBuilderBase>();
            Components = new Dictionary<string, IComponentMenuBuilder>();
            ServiceProvider = serviceProvider;
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

        /// <summary>
        /// Finalize the menus.
        /// </summary>
        public void Contribute()
        {
            foreach (var (_, item) in Store)
            {
                item.Contribute();
            }

            foreach (var (_, item) in Components)
            {
                item.Contribute();
            }
        }

        /// <inheritdoc />
        public IComponentMenuBuilder Component(string name)
        {
            if (Components.ContainsKey(name))
                return Components[name];
            var builder = new ConcreteComponentBuilder(this);
            Components.Add(name, builder);
            return builder;
        }

        /// <inheritdoc />
        public IComponentMenu Extend(string name)
        {
            if (!Components.TryGetValue(name, out var menu) || !(menu is ConcreteComponentBuilder menu2))
                throw new InvalidOperationException(
                    $"\"{name}\" is not a pre-defined menu. " +
                    $"Please add it in the corresponding menu first.");
            menu2.Contribute();
            return menu2;
        }

        /// <inheritdoc />
        public ISubmenu UserDropdown()
        {
            if (!Store.TryGetValue(MenuNameDefaults.UserDropdown, out var menu) || !(menu is ConcreteSubmenuBuilder menu2))
                throw new InvalidOperationException($"\"{MenuNameDefaults.UserDropdown}\" is missing. ");
            menu2.Contribute();
            return menu2;
        }
    }
}
