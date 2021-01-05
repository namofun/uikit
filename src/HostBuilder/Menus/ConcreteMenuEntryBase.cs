using Microsoft.AspNetCore.Mvc.Rendering;
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
        private Func<ViewContext, bool>? Require { get; set; }

        /// <summary>
        /// The internal activity function.
        /// </summary>
        private Func<ViewContext, bool>? Activity { get; set; }

        /// <inheritdoc />
        public Dictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public List<Expression<Func<ViewContext, bool>>> Requirements { get; } = new List<Expression<Func<ViewContext, bool>>>();

        /// <inheritdoc />
        public List<Expression<Func<ViewContext, bool>>> Activities { get; } = new List<Expression<Func<ViewContext, bool>>>();

        /// <inheritdoc />
        string IMenuEntryBase.Title => (string)Metadata[nameof(IMenuEntryBase.Title)];

        /// <inheritdoc />
        string IMenuEntryBase.Icon => (string)Metadata[nameof(IMenuEntryBase.Icon)];

        /// <inheritdoc />
        IEnumerable<(string, BootstrapColor)> IMenuEntryBase.Badges => (IEnumerable<(string, BootstrapColor)>)Metadata.GetValueOrDefault(nameof(IMenuEntryBase.Badges), Enumerable.Empty<(string, BootstrapColor)>())!;
        
        /// <inheritdoc />
        public abstract bool Finalized { get; }

        /// <inheritdoc />
        public string Id { get; private set; } = "menu_" + Guid.NewGuid().ToString("N").Substring(0, 6);

        /// <inheritdoc />
        public virtual void Contribute()
        {
            foreach (var section in new[] { "Title", "Icon", "Link" }.Concat(ToCheck))
                if (!Metadata.ContainsKey(section))
                    throw new ArgumentException(section + " should be set.");
            if (Metadata.ContainsKey("Id")) Id = (string)Metadata["Id"];
            Require = Requirements.CombineAndAlso().Compile();
            Activity = Activities.CombineOrElse().Compile();
        }

        /// <inheritdoc />
        string IMenuEntryBase.GetLink(IUrlHelper urlHelper, ViewContext actionContext)
        {
            return Metadata["Link"] switch
            {
                RouteValueDictionary rvd => urlHelper.Action(new Routing.UrlActionContext { Values = rvd }),
                string rawLink => rawLink,
                Func<IUrlHelper, ViewContext, string> factory => factory(urlHelper, actionContext),
                _ => throw new NotSupportedException("The type of link " + Metadata["Link"].GetType().FullName + " is not supported."),
            };
        }

        /// <inheritdoc />
        bool IMenuEntryBase.Satisfy(ViewContext actionContext)
        {
            return Require!.Invoke(actionContext);
        }

        /// <inheritdoc />
        public bool IsActive(ViewContext actionContext)
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
            entries.ForEach(a => a.Contribute());
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
