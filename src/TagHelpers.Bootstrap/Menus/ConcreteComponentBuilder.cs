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

        /// <summary>
        /// Construct a component builder.
        /// </summary>
        public ConcreteComponentBuilder()
        {
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
        public async Task<IHtmlContent> RenderAsync(IViewComponentHelper helper)
        {
            Contribute();
            if (Components.Count == 0)
                return HtmlString.Empty;
            else if (Components.Count == 1)
                return await helper.InvokeAsync(Components.First().Item2, param);
            var builder = new HtmlContentBuilder(Components.Count);
            foreach (var (_, item) in Components)
                builder.AppendHtml(await helper.InvokeAsync(item, param));
            return builder;
        }
    }
}
