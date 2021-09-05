using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.SmokeTests;
using SatelliteSite.Models;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SatelliteSite.Substrate.Apis
{
    [Area("Dashboard")]
    [Authorize(AuthenticationSchemes = "Basic,Identity.Application")]
    [Authorize(Policy = "HasDashboard")]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public class FabricController : ApiControllerBase
    {
        /// <summary>
        /// Get the loaded components for the system
        /// </summary>
        /// <param name="versions"></param>
        /// <response code="200">Returns the loaded components for the system</response>
        [HttpGet]
        public Task<SystemComponent> Versions(
            [FromServices] ISmokeTest<SystemComponent> versions)
        {
            return versions.GetAsync();
        }


        /// <summary>
        /// Get the enabled endpoints for the system
        /// </summary>
        /// <param name="endpoints"></param>
        /// <response code="200">Returns the enabled endpoints for the system</response>
        [HttpGet]
        public Task<List<RoutingGroup>> Endpoints(
            [FromServices] ISmokeTest<List<RoutingGroup>> endpoints)
        {
            return endpoints.GetAsync();
        }


        /// <summary>
        /// Get the auditlogs for the system
        /// </summary>
        /// <param name="auditlogger"></param>
        /// <param name="page">The page ID</param>
        /// <param name="countPerPage">The count to take per page</param>
        /// <param name="cid">The contest ID</param>
        /// <response code="200">Returns the auditlogs for the system</response>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<PagedResponse<Entities.Auditlog>>> Auditlogs(
            [FromServices] IAuditlogger auditlogger,
            [FromQuery] int page = 1,
            [FromQuery] int countPerPage = 1000,
            [FromQuery] int? cid = null)
        {
            if (page <= 0 || countPerPage <= 0) return BadRequest();
            return new PagedResponse<Entities.Auditlog>(
                await auditlogger.ViewLogsAsync(cid, page, countPerPage));
        }


        /// <summary>
        /// Get the configurations for the system
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="name">The config name</param>
        /// <response code="200">Returns the configurations for the system</response>
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public Task<List<Entities.Configuration>> Config(
            [FromServices] IConfigurationRegistry registry,
            [FromQuery] string? name = null)
        {
            return registry.GetAsync(name);
        }


        /// <summary>
        /// Update the configurations for the system
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="changeItems">The changes to make.</param>
        /// <response code="200">Returns the update results of configurations</response>
        [HttpPatch]
        [Authorize(Roles = "Administrator")]
        [AuditPoint(AuditlogType.Configuration)]
        public async Task<ActionResult<GeneralResponse>> Config(
            [FromServices] IConfigurationRegistry registry,
            [FromBody] Dictionary<string, string> changeItems)
        {
            changeItems ??= new Dictionary<string, string>();
            var items = await registry.GetAsync();
            var pendingChanges = new List<(Entities.Configuration, string)>();

            foreach (var item in items)
            {
                if (!changeItems.TryGetValue(item.Name, out var origVal))
                {
                    continue;
                }

                if (item.Type == "int")
                {
                    if (int.TryParse(origVal, out int origInt))
                    {
                        origVal = origInt.ToString();
                    }
                    else
                    {
                        return new GeneralResponse
                        {
                            Success = false,
                            Reason = $"Unknown integer representation for property {item.Name}.",
                        };
                    }
                }

                var newVal = item.Type switch
                {
                    "string" => (origVal ?? string.Empty).ToJson(),
                    "int" => origVal,
                    "bool" => (origVal == "on" || origVal == "true" || origVal == "yes" || origVal == "1") ? "true" : "false",
                    _ => throw new NotSupportedException(),
                };

                if (newVal != item.Value)
                {
                    pendingChanges.Add((item, newVal));
                }
            }

            if (pendingChanges.Count == 0)
            {
                return new GeneralResponse
                {
                    Success = true,
                    Reason = "No property are changed.",
                };
            }

            foreach (var (conf, val) in pendingChanges)
            {
                await registry.UpdateAsync(conf.Name, val);
                await HttpContext.AuditAsync("updated", conf.Name, $"from {conf.Value} to {val}");
            }

            return new GeneralResponse
            {
                Reason = $"Successfully updated {pendingChanges.Count} entries.",
                Success = true,
            };
        }
    }
}
