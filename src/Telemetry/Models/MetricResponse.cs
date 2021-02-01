using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SatelliteSite.TelemetryModule.Models
{
    public class MetricResponse
    {
        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }

        [JsonPropertyName("ticks")]
        public long[] CommonTick { get; set; }

        [JsonPropertyName("interval")]
        public string Interval { get; set; }

        [JsonPropertyName("segments")]
        public List<AnalyticSegment> Segments { get; set; }
    }

    public class AnalyticSegment
    {
        const long _realSpan = 1000 * 60 * 15;

        [JsonPropertyName("start")]
        public DateTimeOffset Start { get; set; }

        [JsonPropertyName("end")]
        public DateTimeOffset End { get; set; }

        [JsonPropertyName("tick")]
        public long? Tick { get; set; }

        [JsonPropertyName("requests/count")]
        public SumValue RequestsCount { get; set; }

        [JsonPropertyName("requests/failed")]
        public SumValue RequestsFailed { get; set; }

        [JsonPropertyName("requests/duration")]
        public AvgValue RequestsDuration { get; set; }

        [JsonPropertyName("exceptions/server")]
        public SumValue ExceptionsServer { get; set; }

        [JsonPropertyName("dependencies/failed")]
        public SumValue DependenciesFailed { get; set; }

        [JsonPropertyName("performanceCounters/processCpuPercentage")]
        public AvgMaxMinValue ProcessCpuPercentage { get; set; }

        [JsonPropertyName("availabilityResults/availabilityPercentage")]
        public AvgValue AvailabilityPercentage { get; set; }

        [JsonPropertyName("sessions/count")]
        public UniqueValue SessionsCount { get; set; }

        [JsonPropertyName("users/count")]
        public UniqueValue UsersCount { get; set; }

        public AnalyticSegment() { }

        public AnalyticSegment(DateTimeOffset start, DateTimeOffset end)
        {
            Start = start;
            End = end;
            var tick = (start.ToUnixTimeMilliseconds() + end.ToUnixTimeMilliseconds()) / 2;
            if (tick % (2 * _realSpan) != _realSpan)
                tick = tick / (2 * _realSpan) * (2 * _realSpan) + _realSpan;
            Tick = tick;
        }

        public void Combine(AnalyticSegment other)
        {
            this.RequestsCount ??= other.RequestsCount;
            this.RequestsFailed ??= other.RequestsFailed;
            this.RequestsDuration ??= other.RequestsDuration;
            this.ExceptionsServer ??= other.ExceptionsServer;
            this.DependenciesFailed ??= other.DependenciesFailed;
            this.ProcessCpuPercentage ??= other.ProcessCpuPercentage;
            this.AvailabilityPercentage ??= other.AvailabilityPercentage;
            this.SessionsCount ??= other.SessionsCount;
            this.UsersCount ??= other.UsersCount;
        }

        public void FillUp()
        {
            this.RequestsCount ??= new SumValue();
            this.RequestsFailed ??= new SumValue();
            this.ExceptionsServer ??= new SumValue();
            this.DependenciesFailed ??= new SumValue();
            this.AvailabilityPercentage ??= new AvgValue();
            this.SessionsCount ??= new UniqueValue();
            this.UsersCount ??= new UniqueValue();
        }

        public class SumValue
        {
            [JsonPropertyName("sum")]
            public int Sum { get; set; }
        }

        public class UniqueValue
        {
            [JsonPropertyName("unique")]
            public int Unique { get; set; }
        }

        public class AvgValue
        {
            [JsonPropertyName("avg")]
            public double Average { get; set; }
        }

        public class AvgMaxMinValue
        {
            [JsonPropertyName("avg")]
            public double Average { get; set; }

            [JsonPropertyName("max")]
            public double Maximum { get; set; }

            [JsonPropertyName("min")]
            public double Minimum { get; set; }
        }
    }

    public class MetricResponseWrapper
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("body")]
        public MetricResponseBody Body { get; set; }

        public class MetricResponseBody
        {
            [JsonPropertyName("value")]
            public MetricResponse Value { get; set; }
        }
    }
}
