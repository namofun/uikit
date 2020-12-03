using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.DataTables
{
    /// <summary>
    /// An action result which formats the datatable response.
    /// </summary>
    /// <typeparam name="T">The corresponding showing type</typeparam>
    public class DataTableAjaxResult<T> : JsonResult
    {
        /// <summary>
        /// Creates a new <see cref="DataTableAjaxResult{T}"/> with given value.
        /// </summary>
        /// <param name="data">The enumerable representing the data</param>
        /// <param name="draw">The click id from ajax request</param>
        /// <param name="count">The count of total records</param>
        public DataTableAjaxResult(IEnumerable<T> data, int draw, int count) : base(new { draw, recordsTotal = count, recordsFiltered = count, data })
        {
        }

        /// <summary>
        /// Creates a new <see cref="DataTableAjaxResult{T}"/> with given value.
        /// </summary>
        /// <param name="data">The enumerable representing the data</param>
        /// <param name="draw">The click id from ajax request</param>
        /// <param name="count">The count of total records</param>
        /// <param name="serializerSettings">
        /// The serializer settings to be used by the formatter.
        /// <list type="bullet">When using <see cref="System.Text.Json"/>, this should be an instance of <see cref="System.Text.Json.JsonSerializerOptions"/>.</list>
        /// <list type="bullet">When using <see cref="Newtonsoft.Json"/>, this should be an instance of <see cref="Newtonsoft.Json.JsonSerializerSettings"/>.</list>
        /// </param>
        public DataTableAjaxResult(IEnumerable<T> data, int draw, int count, object serializerSettings) : base(new { draw, recordsTotal = count, recordsFiltered = count, data }, serializerSettings)
        {
        }
    }
}
