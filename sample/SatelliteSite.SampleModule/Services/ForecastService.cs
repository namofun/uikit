using SatelliteSite.SampleModule.Models;
using SatelliteSite.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SatelliteSite.SampleModule.Services
{
    public class ForecastService
    {
        IConfigurationRegistry Registry { get; }

        public ForecastService(IConfigurationRegistry configuration)
        {
            Registry = configuration;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public IEnumerable<WeatherForecast> Forecast()
        {
            int count = Registry.GetAsync("random_count").Result.Single().Value.AsJson<int>();
            var rng = new Random();
            return Enumerable.Range(1, count).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
