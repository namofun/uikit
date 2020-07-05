using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <inheritdoc cref="IMenuBuilder" />
    internal class ConcreteMenuBuilder : IMenuBuilder, IMenu
    {
        /// <summary>
        /// The menu entries.
        /// </summary>
        public List<IMenuEntryBase> Entries { get; }

        /// <summary>
        /// The parent contributor.
        /// </summary>
        public ConcreteMenuContributor Contributor { get; }

        /// <summary>
        /// The names of submenus.
        /// </summary>
        public HashSet<string> Submenus { get; }

        /// <inheritdoc />
        public Dictionary<string, object> Metadata { get; }

        /// <inheritdoc />
        public List<Expression<Func<HttpContext, bool>>> Requirements { get; }

        /// <inheritdoc />
        public bool Finalized { get; set; }

        /// <inheritdoc />
        public int Count => Entries.Count;

        /// <inheritdoc />
        public List<Expression<Func<ViewContext, bool>>> Activities => throw new NotImplementedException();

        /// <inheritdoc />
        public IMenuEntryBase this[int index] => Entries[index];

        /// <summary>
        /// Initiate the <see cref="IMenuBuilder"/>.
        /// </summary>
        /// <param name="contributor">The <see cref="IMenuContributor"/></param>
        public ConcreteMenuBuilder(ConcreteMenuContributor contributor)
        {
            Contributor = contributor;
            Requirements = new List<Expression<Func<HttpContext, bool>>>();
            Metadata = new Dictionary<string, object>();
            Submenus = new HashSet<string>();
            Entries = new List<IMenuEntryBase>();
        }

        /// <inheritdoc />
        public void HasSubmenu(int priority, Action<ISubmenuBuilder> configure)
        {
            ((IMenuBuilder)this).HasSubmenu(Guid.NewGuid().ToString(), priority, configure);
        }

        /// <inheritdoc />
        public IMenuEntryBuilder HasEntry(int priority)
        {
            var entry = new ConcreteMenuEntryBuilder { Priority = priority };
            Entries.Add(entry);
            return entry;
        }

        /// <inheritdoc />
        void IMenuBuilder.HasSubmenu(string name, int priority, Action<ISubmenuBuilder> configure)
        {
            if (!Submenus.Contains(name)) Submenus.Add(name);
            ISubmenuBuilder menu2;
            if (!Contributor.Store.TryGetValue(name, out var menu))
                Contributor.Store.Add(name, menu2 = new ConcreteSubmenuBuilder { Priority = priority });
            else if (menu is ISubmenuBuilder menu3)
                menu2 = menu3;
            else
                throw new InvalidOperationException(
                    $"\"{name}\" is not a submenu.");
            configure.Invoke(menu2);
        }

        /// <inheritdoc />
        public void Contribute()
        {
            if (Finalized) return;
            Finalized = true;

            var subMenus = Contributor.Store
                .Where(a => Submenus.Contains(a.Key))
                .Select(a => a.Value)
                .Cast<ConcreteSubmenuBuilder>()
                .ToList();

            subMenus.ForEach(a => a.Contribute());
            Entries.ForEach(a => ((IMenuEntryBuilder)a).Contribute());
            Entries.AddRange(subMenus);
            Entries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        /// <inheritdoc />
        public IEnumerator<IMenuEntryBase> GetEnumerator()
        {
            return Entries.GetEnumerator();
        }
    }
}
