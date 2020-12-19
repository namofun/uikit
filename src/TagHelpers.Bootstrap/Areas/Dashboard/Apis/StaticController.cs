using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using SatelliteSite.Entities;
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
            [FromServices] IMediator mediator)
        {
            if (formFile == null || type == null)
                return new ObjectResult(new { success = 0, message = "未包含文件。" });
            if (formFile.Length > (5 << 20))
                return new ObjectResult(new { success = 0, message = "文件过大。" });

            // handle authorize
            var request = new ImageUploadPermission(HttpContext, id, type, formFile);
            await mediator.Publish(request);

            if (request.Handled == false)
                return new ObjectResult(new { success = 0, message = "无权限访问。" });
            if (request.Handled == null)
                return new ObjectResult(new { success = 0, message = "权限暂未确认。" });

            // upload files
            try
            {
                string fileName, fileNameFull;
                do
                {
                    var ext = Path.GetExtension(formFile.FileName);
                    var guid = Guid.NewGuid().ToString("N").Substring(0, 16);
                    fileName = $"{type}{id}.{guid}{ext}";
                    fileNameFull = "images/problem/" + fileName;
                }
                while ((await io.GetFileInfoAsync(fileNameFull)).Exists);

                using (var content = formFile.OpenReadStream())
                    await io.WriteStreamAsync(fileNameFull, content);

                await HttpContext.AuditAsync("upload", fileName);
                return new ObjectResult(new { success = 1, url = "/" + fileNameFull });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { success = 0, message = "内部错误。" + ex.Message });
            }
        }
    }
}
