using Microsoft.AspNetCore.Mvc;
using SatelliteSite.SampleModule.Models;
using SatelliteSite.SampleModule.Services;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SatelliteSite.SampleModule.Apis
{
    [Area("Api")]
    [Route("[area]/[controller]")]
    [Produces("application/json")]
    public class WeatherController : ApiControllerBase
    {
        private ForecastService Service { get; }

        public WeatherController(ForecastService service)
        {
            Service = service;
        }

        
        /// <summary>
        /// Get all the weather forecasts
        /// </summary>
        /// <response code="200">Returns all the weather forecasts</response>
        [HttpGet]
        public ActionResult<WeatherForecast[]> GetAll()
        {
            return Service.Forecast().ToArray();
        }


        /// <summary>
        /// Get the given weather forecast
        /// </summary>
        /// <param name="id">The ID of entity to get</param>
        /// <response code="200">Returns the given weather forecast</response>
        [HttpGet("{id}")]
        public ActionResult<WeatherForecast> GetOne(
            [FromRoute, Required]string id)
        {
            return Service.Forecast().First();
        }


        /// <summary>
        /// Create a new weather forecast
        /// </summary>
        /// <response code="201">Returns the created weather forecast</response>
        [HttpPost]
        public ActionResult<WeatherForecast> CreateOne(
            [Required] WeatherForecast model)
        {
            return CreatedAtRoute(new { id = "1" }, model);
        }


        /// <summary>
        /// Replace the given weather forecast
        /// </summary>
        /// <param name="id">The ID of entity to replace</param>
        /// <response code="200">Returns the replaced weather forecast</response>
        [HttpPut("{id}")]
        public ActionResult<WeatherForecast> ReplaceOne(
            [FromRoute, Required] string id,
            [Required] WeatherForecast model)
        {
            return Ok(model);
        }


        /// <summary>
        /// Change the given weather forecast
        /// </summary>
        /// <param name="id">The ID of entity to change</param>
        /// <response code="200">Returns the changed weather forecast</response>
        [HttpPatch("{id}")]
        public ActionResult<WeatherForecast> ChangeOne(
            [FromRoute, Required] string id)
        {
            return Ok(Service.Forecast().First());
        }


        /// <summary>
        /// Delete the given weather forecast
        /// </summary>
        /// <param name="id">The ID of entity to delete</param>
        /// <response code="204">Returns the deleted weather forecast</response>
        [HttpDelete("{id}")]
        public NoContentResult DeleteOne(
            [FromRoute, Required] string id)
        {
            return NoContent();
        }
    }
}
