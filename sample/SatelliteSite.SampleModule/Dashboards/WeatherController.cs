using Jobs.Services;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.SampleModule.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[controller]")]
    public class WeatherController : ViewControllerBase
    {
        private ForecastService Service { get; }

        public WeatherController(ForecastService service)
        {
            Service = service;
        }


        [HttpGet("[action]")]
        public IActionResult Change()
        {
            return View(Service.Forecast());
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> CreateJob(
            [FromServices] IJobScheduler scheduler)
        {
            var j = await scheduler.ScheduleAsync(new Jobs.Models.JobDescription
            {
                Arguments = Service.Forecast().ToJson(),
                JobType = "Sample.PingPong",
                SuggestedFileName = "ping-pong.json",
                OwnerId = int.Parse(User.GetUserId()),
            });

            return RedirectToAction("Detail", "Jobs", new { area = "Dashboard", id = j.JobId });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> CreateJob2(
            [FromServices] IJobScheduler scheduler)
        {
            var children = new List<Jobs.Models.JobDescription>();
            var owner = int.Parse(User.GetUserId());

            for (int i = 0; i < 10; i++)
            {
                children.Add(new Jobs.Models.JobDescription
                {
                    Arguments = Service.Forecast().ToJson(),
                    JobType = "Sample.PingPong",
                    SuggestedFileName = "day" + i + ".json",
                    OwnerId = int.Parse(User.GetUserId()),
                });
            }

            var total = Jobs.Works.ComposeArchive.ForChildren(owner, "day0-9.zip", children);
            var j = await scheduler.ScheduleAsync(total);

            return RedirectToAction("Detail", "Jobs", new { area = "Dashboard", id = j.JobId });
        }
    }
}
