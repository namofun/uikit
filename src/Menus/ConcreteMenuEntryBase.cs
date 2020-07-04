using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <summary>
    /// The default implement for <see cref="IMenuEntryBase"/> and <see cref="IMenuEntryBuilderBase"/>.
    /// </summary>
    internal abstract class ConcreteMenuEntryBase : IMenuEntryBase, IMenuEntryBuilderBase
    {
        /// <summary>
        /// The properties to check.
        /// </summary>
        public abstract string[] ToCheck { get; }

        /// <inheritdoc />
        public int Priority { get; set; }

        /// <summary>
        /// The internal require function.
        /// </summary>
        private Func<HttpContext, bool>? Require { get; set; }

        /// <summary>
        /// The internal activity function.
        /// </summary>
        private Func<ActionContext, bool>? Activity { get; set; }

        /// <summary>
        /// The activity list.
        /// </summary>
        IReadOnlyList<(string, string?, string?)> ActivityList => (IReadOnlyList<(string, string?, string?)>)Metadata.GetValueOrDefault(nameof(MenuNameDefaults.ActiveWhen), Array.Empty<(string, string?, string?)>())!;

        /// <inheritdoc />
        public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public List<Expression<Func<HttpContext, bool>>> Requirements { get; } = new List<Expression<Func<HttpContext, bool>>>();

        /// <inheritdoc />
        string IMenuEntryBase.Title => (string)Metadata[nameof(IMenuEntryBase.Title)];

        /// <inheritdoc />
        string IMenuEntryBase.Icon => (string)Metadata[nameof(IMenuEntryBase.Icon)];

        /// <inheritdoc />
        IEnumerable<(string, BootstrapColor)> IMenuEntryBase.Badges => (IEnumerable<(string, BootstrapColor)>)Metadata.GetValueOrDefault(nameof(IMenuEntryBase.Badges), Enumerable.Empty<(string, BootstrapColor)>())!;
        
        /// <inheritdoc />
        public abstract bool Finalized { get; }

        /// <summary>
        /// Checks whether this <see cref="ControllerActionDescriptor"/> satisfy.
        /// </summary>
        /// <param name="cc">The <see cref="ActionDescriptor"/>.</param>
        /// <param name="items">The (area, controller, action).</param>
        /// <returns>The check result.</returns>
        static bool Checks(ActionDescriptor cc, (string, string?, string?) items)
        {
            if (!(cc is ControllerActionDescriptor c)) return false;
            var (area, controller, action) = items;
            if (c.RouteValues.TryGetValue("area", out var a2) && a2 != area)
                return false;
            if (controller != null && c.ControllerName != controller)
                return false;
            if (action != null && c.ActionName != controller)
                return false;
            return true;
        }

        /// <inheritdoc />
        public virtual void Contribute()
        {
            foreach (var section in new[] { "Title", "Icon", "Link" }.Concat(ToCheck))
                if (!Metadata.ContainsKey(section))
                    throw new ArgumentException(section + " should be set.");
            Require = Requirements.CombineAndAlso().Compile();
            Activity = ActivityList.Select(Factory).ToList().CombineOrElse().Compile();

            static Expression<Func<ActionContext, bool>> Factory((string, string?, string?) items)
            {
                var (area, controller, action) = items;
                if (area == null || (controller == null && action != null))
                    throw new InvalidOperationException($"Error arguments specified: {items}.");
                return c => Checks(c.ActionDescriptor, items);
            }
        }

        /// <inheritdoc />
        string IMenuEntryBase.GetLink(IUrlHelper urlHelper)
        {
            var rvd = (RouteValueDictionary)Metadata["Link"];
            if (rvd.Count == 1 && rvd.ContainsKey("link"))
                return (string)rvd["link"];

            if (rvd.Count == 3)
            {
                var ctrl = (string)rvd["controller"];
                var act = (string)rvd["action"];
                var area = (string)rvd["area"];
                return urlHelper.Action(act, ctrl, new { area });
            }

            throw new NotImplementedException();
        }

        /// <inheritdoc />
        bool IMenuEntryBase.Satisfy(HttpContext httpContext)
        {
            return Require!.Invoke(httpContext);
        }

        public bool IsActive(ActionContext actionContext)
        {
            return Activity!.Invoke(actionContext);
        }
    }

    /// <inheritdoc cref="IMenuEntryBuilder" />
    internal class ConcreteMenuEntryBuilder : ConcreteMenuEntryBase, IMenuEntryBuilder, IMenuEntry
    {
        /// <inheritdoc />
        public override string[] ToCheck => Array.Empty<string>();

        /// <summary>
        /// Whether this entry is finalized.
        /// </summary>
        private bool finalized;

        /// <inheritdoc />
        public override void Contribute()
        {
            if (finalized) return;
            finalized = true;
            base.Contribute();
        }

        /// <inheritdoc />
        public override bool Finalized => finalized;
    }

    /// <inheritdoc cref="ISubmenuBuilder" />
    internal class ConcreteSubmenuBuilder : ConcreteMenuEntryBase, ISubmenuBuilder, ISubmenu
    {
        /// <summary>
        /// The internal entries.
        /// </summary>
        private readonly List<ConcreteMenuEntryBuilder> entries;

        /// <summary>
        /// Whether this entry is finalized.
        /// </summary>
        private bool finalized;

        /// <summary>
        /// Instantiate the <see cref="ISubmenuBuilder"/>.
        /// </summary>
        public ConcreteSubmenuBuilder()
        {
            entries = new List<ConcreteMenuEntryBuilder>();
        }

        /// <inheritdoc />
        public IMenuEntry this[int index] => entries[index];

        /// <inheritdoc />
        public override string[] ToCheck => Array.Empty<string>();

        /// <inheritdoc />
        public int Count => entries.Count;

        /// <inheritdoc />
        public override bool Finalized => finalized;

        /// <inheritdoc />
        public IEnumerator<IMenuEntry> GetEnumerator() => entries.GetEnumerator();

        /// <inheritdoc />
        public override void Contribute()
        {
            if (finalized) return;
            finalized = true;
            base.Contribute();
            entries.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        /// <inheritdoc />
        public IMenuEntryBuilder HasEntry(int priority)
        {
            if (finalized) throw new InvalidOperationException();
            var entry = new ConcreteMenuEntryBuilder { Priority = priority };
            entries.Add(entry);
            return entry;
        }
    }
}
