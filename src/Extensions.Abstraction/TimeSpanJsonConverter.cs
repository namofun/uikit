namespace System.Text.Json.Serialization
{
    /// <summary>
    /// Converts a <see cref="TimeSpan"/> to or from JSON.
    /// </summary>
    public class TimeSpanJsonConverter : JsonConverter<TimeSpan>
    {
        /// <inheritdoc />
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // explicitly suppress the null because it will throws
            return TimeSpan.Parse(reader.GetString()!);
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            string op = "-";
            if (value < TimeSpan.Zero) value = -value;
            else op = "";
            writer.WriteStringValue($"{op}{value.Days * 24 + value.Hours}:{value.Minutes:d2}:{value.Seconds:d2}.{value.Milliseconds:d3}");
        }
    }
}
