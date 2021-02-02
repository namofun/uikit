using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.DataTables.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    /// <summary>
    /// Render a table using <c>dataTables</c>.
    /// </summary>
    [HtmlTargetElement("datatable", TagStructure = TagStructure.WithoutEndTag)]
    public class DataTablesTagHelper : TagHelper
    {
        /// <summary>
        /// The cache to store factories for HtmlContent
        /// </summary>
        private static readonly IMemoryCache FactoryCache =
            new MemoryCache(new MemoryCacheOptions { Clock = new Extensions.Internal.SystemClock() });

        /// <summary>
        /// The core typed data to read from
        /// </summary>
        [HtmlAttributeName("data")]
        public IEnumerable? Data { get; set; }

        /// <summary>
        /// The element type from <c>Data</c> property
        /// </summary>
        [HtmlAttributeName("element-type")]
        public Type? DataType { get; set; }

        /// <summary>
        /// The ajax url to update from
        /// </summary>
        [HtmlAttributeName("asp-url")]
        public string? UpdateUrl { get; set; }

        /// <summary>
        /// The page length (if <c>null</c>, then no pagination)
        /// </summary>
        [HtmlAttributeName("paging")]
        public int? Paging { get; set; }

        /// <summary>
        /// Whether the table is auto width
        /// </summary>
        [HtmlAttributeName("auto-width")]
        public bool AutoWidth { get; set; } = true;

        /// <summary>
        /// The CSS class for thead elements
        /// </summary>
        [HtmlAttributeName("thead-class")]
        public string? TableHeaderClass { get; set; }

        /// <summary>
        /// Output the corresponding DataTable scripts without ajax support
        /// </summary>
        /// <param name="uniqueId">The table unique ID</param>
        /// <param name="model">The dataTable view model</param>
        /// <param name="content">The <see cref="TagHelperContent"/> to write to</param>
        private void PrintScript(
            string uniqueId,
            DataTableViewModel model,
            TagHelperContent content)
        {
            content.AppendHtml("\r\n<script>$().ready(function(){$('#");
            content.Append(uniqueId);
            content.AppendHtml("').DataTable({ ");
            content.AppendHtml("'searching': " + (model.Searchable ? "true" : "false") + ", ");

            if (model.Sortable)
                content.AppendHtml("'ordering': true, 'order': [" + model.Sort + "], ");
            else
                content.AppendHtml("'ordering': false, ");

            if (Paging.HasValue)
                content.AppendHtml("'paging': true, 'pageLength': " + Paging.Value + ", 'lengthChange': false, ");
            else
                content.AppendHtml("'paging': false, ");

            content.AppendHtml("'info': false, 'autoWidth': " + (AutoWidth ? "true" : "false") + ", ");
            content.AppendHtml("'language': { 'searchPlaceholder': 'filter table', 'search': '_INPUT_', 'oPaginate': {'sPrevious': '&laquo;', 'sNext': '&raquo;'} }, ");
            content.AppendHtml("'aoColumnDefs': [{ aTargets: ['sortable'], bSortable: true }, { aTargets: ['searchable'], bSearchable: true }, { aTargets: ['_all'], bSortable: false, bSearchable: false }], ");

            if (UpdateUrl != null)
            {
                content.AppendHtml("'serverSide': true, 'ajax': { 'url': '" + UpdateUrl + "' }, ");
                content.AppendHtml(model.Scripts);
            }

            content.AppendHtmlLine("});});</script>");
        }

        /// <summary>
        /// Process the data table building.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        /// <param name="output">A stateful HTML element used to generate an HTML tag.</param>
        /// <returns>A <see cref="Task"/> that on completion updates the output.</returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            await base.ProcessAsync(context, output);
            output.Attributes.SetAttribute("id", context.UniqueId);

            if (Data == null && DataType == null)
                throw new InvalidOperationException("No data specified.");
            if (Data != null)
                DataType = Data.GetType().GetInterface("IEnumerable`1")?.GetGenericArguments()[0]
                    ?? throw new NotSupportedException();

            var viewModel = await FactoryCache.GetOrCreateAsync(DataType,
                entry => DataRowFunctions.Factory((Type)entry.Key));

            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.AddClass("data-table table table-sm table-striped");
            output.PreElement.AppendHtml("<div class=\"table-wrapper\">");
            output.PostElement.AppendHtml("</div>");
            PrintScript(context.UniqueId, viewModel, output.PostElement);
            if (AutoWidth) output.Attributes.Add("style", "width:auto");

            var thead = new TagBuilder("thead").WithBody(viewModel.THead);
            if (TableHeaderClass != null) thead.AddCssClass(TableHeaderClass);
            output.Content.AppendHtml("\r\n").AppendHtml(thead);

            if (Data != null)
            {
                var tbody = new TagBuilder("tbody");

                foreach (var item in Data)
                {
                    if (item == null)
                    {
                        tbody.InnerHtml.SetHtmlContent("<tr><td>NULL error</td></tr>");
                        break;
                    }
                    else
                    {
                        var tr = viewModel.TRow(item);
                        tr.MergeAttribute("role", "row");
                        tbody.InnerHtml.AppendHtml("\r\n").AppendHtml(tr);
                    }
                }

                output.Content.AppendHtml(tbody);
            }
        }
    }
}
