using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.SmokeTests;
using SatelliteSite.Services;
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
        public async Task<ActionResult<PagedResponse<Entities.Auditlog>>> Auditlog(
            [FromServices] IAuditlogger auditlogger,
            [FromQuery] int page = 1,
            [FromQuery] int countPerPage = 1000,
            [FromQuery] int? cid = null)
        {
            if (page <= 0 || countPerPage <= 0) return BadRequest();
            return new PagedResponse<Entities.Auditlog>(
                await auditlogger.ViewLogsAsync(cid, page, countPerPage));
        }
    }
}
