﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SatelliteSite.Substrate.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[action]")]
    [Authorize("HasDashboard")]
    [SupportStatusCodePage]
    public class RootController : ViewControllerBase
    {
        [HttpGet("/[area]", Order = 100, Name = "DashboardIndex")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Endpoints(
            [FromServices] CompositeEndpointDataSource endpointDataSource,
            [FromServices] ReExecuteEndpointDataSource endpointDataSource2)
        {
            var edss = endpointDataSource.DataSources;
            if (endpointDataSource2.Endpoints.Count > 0)
                edss = edss.Append(endpointDataSource2);
            return View(edss);
        }


        [HttpGet]
        public async Task<IActionResult> Updates(
            [FromServices] MediatR.IMediator mediator)
        {
            var model = new DashboardUpdates(HttpContext);
            await mediator.Publish(model);
            return Content(model.ToString(), "application/json");
        }


        [HttpGet]
        public IActionResult Versions()
        {
            var lst = new List<LoadedModulesModel>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                GitVersionAttribute? gitVersion;

                try
                {
                    gitVersion = assembly.GetCustomAttribute<GitVersionAttribute>();
                }
                catch
                {
                    gitVersion = null;
                }

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

            if (HttpContext.RequestServices.GetService(typeof(IRazorFileProvider)) != null)
                ViewData["RazorRuntimeCompilationEnabled"] = true;
            return View(lst);
        }


        [HttpGet("{page?}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Auditlog(
            [FromServices] IAuditlogger auditlogger,
            int page = 1)
        {
            if (page <= 0) return BadRequest();
            return View(await auditlogger.ViewLogsAsync(null, page, 1000));
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
                if (item.Type == "bool" &&
                    !models.Config.ContainsKey(item.Name))
                    models.Config.Add(item.Name, "off");

                if (!models.Config.ContainsKey(item.Name)
                    || models.Config[item.Name] == null)
                    continue;

                var origVal = models.Config[item.Name];

                var newVal = item.Type switch
                {
                    "string" => origVal.ToJson(),
                    "int" => origVal,
                    "bool" => (origVal == "on" || origVal == "true" || origVal == "yes" || origVal == "1") ? "true" : "false",
                    _ => throw new NotSupportedException(),
                };

                if (newVal == item.Value) continue;

                await registry.UpdateAsync(item.Name, newVal);
                await HttpContext.AuditAsync("updated", item.Name, "from " + item.Value);
            }

            StatusMessage = "Configurations saved successfully.";
            return RedirectToAction(nameof(Config));
        }
    }
}
