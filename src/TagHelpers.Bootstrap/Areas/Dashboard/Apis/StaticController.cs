using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using SatelliteSite.Substrate.Dashboards;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite.Substrate.Apis
{
    [Area("Dashboard")]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    [AuditPoint(AuditlogType.Attachment)]
    public class StaticController : ApiControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> ImagesUpload(
            [FromQuery] int? id,
            [FromQuery] string type,
            [FromForm(Name = "editormd-image-file")] IFormFile formFile,
            [FromServices] IWwwrootFileProvider io,
            [FromServices] MediatR.IMediator mediator,
            [FromServices] IStringLocalizer<StaticController> localizer)
        {
            if (formFile == null || type == null) return Fail("No files were uploaded.");
            if (formFile.Length > (5 << 20)) return Fail("The uploaded file is too big.");

            // handle authorize
            var request = new ImageUploadPermission(HttpContext, id, type, formFile);
            await mediator.Publish(request);

            if (request.Handled == false) return Fail("Permission Denied.");
            if (request.Handled == null) return Fail("Permission is not checked yet.");

            // upload files
            try
            {
                string fileName, fileNameFull;
                var ext = Path.GetExtension(formFile.FileName);

                do
                {
                    var guid = Guid.NewGuid().ToString("N").Substring(0, 16);
                    fileName = $"{type}{id}.{guid}{ext}";
                    fileNameFull = "images/problem/" + fileName;
                }
                while ((await io.GetFileInfoAsync(fileNameFull)).Exists);

                using (var content = formFile.OpenReadStream())
                {
                    await io.WriteStreamAsync(fileNameFull, content);
                }

                await HttpContext.AuditAsync("upload", fileName);
                return new ObjectResult(new { success = 1, url = "/" + fileNameFull });
            }
            catch (Exception ex)
            {
                return Fail("Internal Error.", ex.Message);
            }

            ObjectResult Fail(string reason, string? more = null)
            {
                return new ObjectResult(new { success = 0, message = localizer[reason] + more });
            }
        }
    }
}
