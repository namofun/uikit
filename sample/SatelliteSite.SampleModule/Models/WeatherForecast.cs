using Microsoft.AspNetCore.Mvc.DataTables;
using System;

namespace SatelliteSite.SampleModule.Models
{
    public class WeatherForecast
    {
        [DtDisplay(0, "date")]
        public DateTime Date { get; set; }

        [DtDisplay(1, "temperature", "{TemperatureC}°C ({TemperatureF}°F)")]
        public int TemperatureC { get; set; }

        [DtIgnore]
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        [DtDisplay(2, "summary")]
        public string Summary { get; set; }
    }
}
