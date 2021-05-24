using Jobs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.JobsModule.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[controller]")]
    [Authorize(Policy = "HasDashboard")]
    [AuditPoint(AuditlogType.BackgroundJobs)]
    public class JobsController : ViewControllerBase
    {
        private IJobManager Manager { get; }

        public JobsController(IJobManager manager)
        {
            Manager = manager;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page <= 0) return BadRequest();
            int uid = int.Parse(User.GetUserId());
            return View(await Manager.GetJobsAsync(uid, page));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Detail(Guid id)
        {
            int uid = int.Parse(User.GetUserId());
            var job = await Manager.FindJobAsync(id, uid);
            if (job == null) return NotFound();
            return View(job);
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> Logs(Guid id)
        {
            int uid = int.Parse(User.GetUserId());
            var job = await Manager.FindJobAsync(id, uid);
            if (job == null) return NotFound();

            var logs = await Manager.GetLogsAsync(id);
            if (logs == null || !logs.Exists) return StatusCode(503);
            return File(logs.CreateReadStream(), "text/plain", job.SuggestedFileName + ".log", false);
        }


        [HttpGet("{id}/[action]")]
        public async Task<IActionResult> Download(Guid id)
        {
            int uid = int.Parse(User.GetUserId());
            var job = await Manager.FindJobAsync(id, uid);
            if (job == null) return NotFound();

            var file = await Manager.GetDownloadAsync(id);
            if (file == null || !file.Exists) return StatusCode(503);
            return File(file.CreateReadStream(), "application/octet-stream", job.SuggestedFileName, true);
        }
    }
}
