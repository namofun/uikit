﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.Services;
using System.Text;
using System.Threading.Tasks;

namespace SatelliteSite.Substrate.Apis
{
    [Area("Api")]
    [Route("[area]/[action]")]
    public class ConfigurationController : ApiControllerBase
    {
        /// <summary>
        /// Get configuration variables
        /// </summary>
        /// <param name="name">Get only this configuration variable</param>
        /// <param name="configs"></param>
        /// <response code="200">The configuration variables</response>
        [HttpGet]
        [Authorize("ReadConfiguration")]
        public async Task<IActionResult> Config(string? name,
            [FromServices] IConfigurationRegistry configs)
        {
            var value = await configs.GetAsync(name);
            var result = new StringBuilder();
            result.Append("{");
            for (int i = 0; i < value.Count; i++)
                result.Append(i != 0 ? ",\"" : "\"")
                      .Append(value[i].Name)
                      .Append("\":")
                      .Append(value[i].Value);
            result.Append("}");
            return Content(result.ToString(), "application/json");
        }
    }
}
