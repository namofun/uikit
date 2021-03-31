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
        private Func<ViewContext, bool>? Active { get; set; }

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
        public IMenuContributor Contributor { get; }

        /// <summary>
        /// Initialize the <see cref="IMenuEntryBase"/>.
        /// </summary>
        protected ConcreteMenuEntryBase(IMenuContributor menuContributor)
        {
            Contributor = menuContributor;
        }

        /// <inheritdoc />
        public virtual void Contribute()
        {
            foreach (var section in new[] { "Title", "Icon", "Link" }.Concat(ToCheck))
                if (!Metadata.ContainsKey(section))
                    throw new ArgumentException(section + " should be set.");
            if (Metadata.ContainsKey("Id")) Id = (string)Metadata["Id"];
            Require = Requirements.Count == 0 ? null : Requirements.CombineAndAlso().Compile();
            Active = Activities.Count == 0 ? null : Activities.CombineOrElse().Compile();
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
            return Require?.Invoke(actionContext) ?? true;
        }

        /// <inheritdoc />
        public bool IsActive(ViewContext actionContext)
        {
            return Active?.Invoke(actionContext) ?? false;
        }
    }
}
