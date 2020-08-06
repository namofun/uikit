﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using SatelliteSite.Entities;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.Substrate.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[action]")]
    //[Authorize("HasDashboard")]
    public class RootController : ViewControllerBase
    {
        [HttpGet("/[area]")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Endpoints(
            [FromServices] IOptions<RouteOptions> options)
        {
            var edss = options.Value.Private<ICollection<EndpointDataSource>>("_endpointDataSources");
            return View(edss);
        }


        [HttpGet]
        public IActionResult Versions()
        {
            var lst = new List<LoadedModulesModel>();
                
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var gitVersion = assembly.GetCustomAttributes(false)
                    .OfType<GitVersionAttribute>()
                    .SingleOrDefault();
                var asName = assembly.GetName();

                lst.Add(new LoadedModulesModel
                {
                    AssemblyName = asName.Name,
                    Branch = gitVersion?.Branch,
                    CommitLong = gitVersion?.Version,
                    PublicKey = asName.GetPublicKeyToken()?.ToHexDigest(true),
                    Version = asName.Version?.ToString(),
                });
            }

            return View(lst);
        }


        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Config(
            [FromServices] IConfigurationRegistry registry)
        {
            return View(await registry.ListAsync());
        }


        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        [AuditPoint(AuditlogType.Configuration)]
        public async Task<IActionResult> Config(
            ConfigureEditModel models,
            [FromServices] IConfigurationRegistry registry)
        {
            var items = (await registry.ListAsync()).SelectMany(a => a).ToList();

            foreach (var item in items)
            {
                if (!models.Config.ContainsKey(item.Name)
                    || models.Config[item.Name] == null)
                    continue;

                var newVal = models.Config[item.Name];
                if (item.Type == "string") newVal = newVal.ToJson();
                if (newVal == item.Value) continue;

                await registry.UpdateAsync(item.Name, newVal);
                await HttpContext.AuditAsync("updated", item.Name, "from " + item.Value);
            }

            StatusMessage = "Configurations saved successfully.";
            return RedirectToAction(nameof(Config));
        }
    }
}
