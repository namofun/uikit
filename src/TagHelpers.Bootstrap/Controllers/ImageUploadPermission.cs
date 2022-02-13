using MediatR;
using Microsoft.AspNetCore.Http;

namespace SatelliteSite.Substrate.Dashboards
{
    /// <summary>
    /// Consume image upload permission checks.
    /// </summary>
    public class ImageUploadPermission : INotification
    {
        /// <summary>
        /// The HTTP Context
        /// </summary>
        public HttpContext Context { get; }

        /// <summary>
        /// The upload related entity ID
        /// </summary>
        public int? Id { get; }

        /// <summary>
        /// The upload related entity type
        /// </summary>
        public string? Type { get; }

        /// <summary>
        /// The uploaded file
        /// </summary>
        /// <remarks>
        /// The file content shouldn't consume here.
        /// That is saying, <see cref="IFormFile.OpenReadStream"/>, <see cref="IFormFile.CopyToAsync(System.IO.Stream, System.Threading.CancellationToken)"/> shouldn't be invoked.
        /// </remarks>
        public IFormFile? FormFile { get; }

        /// <summary>
        /// The mark for handling this request
        /// </summary>
        public bool? Handled { get; private set; }

        /// <summary>
        /// The category to storage the picture
        /// </summary>
        public string? Category { get; private set; }

        /// <summary>
        /// Produce the notification.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="id">The target ID.</param>
        /// <param name="type">The target type.</param>
        /// <param name="formFile">The form file.</param>
        internal ImageUploadPermission(HttpContext context, int? id, string? type, IFormFile? formFile)
        {
            Context = context;
            Id = id;
            Type = type;
            FormFile = formFile;
        }

        /// <summary>
        /// Rejects the uploading request.
        /// </summary>
        public void Reject()
        {
            Handled = false;
        }

        /// <summary>
        /// Accepts the uploading request with specified category.
        /// </summary>
        /// <param name="category">The sub-directory name of <c>/images/</c> to upload to. Null if upload directly to <c>/images/</c>.</param>
        public void Accept(string? category)
        {
            Handled = true;
            Category = category;
        }
    }
}
