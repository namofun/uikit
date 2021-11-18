using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.Menus
{
    /// <inheritdoc cref="IComponentMenuBuilder" />
    internal class ConcreteComponentBuilder : IComponentMenuBuilder, IComponentMenu
    {
        private static readonly object param = new { };

        /// <inheritdoc />
        public IReadOnlyCollection<(int, Type)> Components { get; private set; }

        /// <inheritdoc />
        public bool Finalized { get; private set; }

        /// <inheritdoc />
        public IMenuContributor Contributor { get; }

        /// <summary>
        /// Construct a component builder.
        /// </summary>
        public ConcreteComponentBuilder(IMenuContributor contributor)
        {
            Contributor = contributor;
            Components = new List<(int, Type)>();
            Finalized = false;
        }

        /// <inheritdoc />
        public void Contribute()
        {
            if (Finalized) return;
            Finalized = true;
            var lst = Components.OrderBy(a => a.Item1).ToList();
            Components = new ReadOnlyCollection<(int, Type)>(lst);
        }

        /// <inheritdoc />
        public async Task<IHtmlContent> RenderAsync(IViewComponentHelper helper, object? model = null)
        {
            model ??= param;
            Contribute();
            if (Components.Count == 0) return HtmlString.Empty;

            var builder = new HtmlContentBuilder(Components.Count * 2);
            foreach (var (order, item) in Components)
            {
                builder.AppendHtml($"<!--#component order={order}-->\r\n");
                builder.AppendHtml(await helper.InvokeAsync(item, model));
            }

            return builder;
        }
    }
}
