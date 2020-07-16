namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Validate the <see cref="TimeSpan"/> property.
    /// </summary>
    public sealed class TimeSpanAttribute : ValidationAttribute
    {
        /// <inheritdoc />
        public override bool IsValid(object value)
        {
            if (value == null) return true;
            if (!(value is string realValue)) return false;
            if (string.IsNullOrEmpty(realValue)) return true;
            return realValue.TryParseAsTimeSpan(out _);
        }

        /// <inheritdoc />
        public override string FormatErrorMessage(string name)
        {
            return $"Error parsing the format of timespan {name}.";
        }
    }
}

namespace System
{
    /// <summary>
    /// The extension methods for parsing <see cref="TimeSpan"/>.
    /// </summary>
    public static class TimeSpanAttributeHelper
    {
        /// <summary>
        /// Try parse as <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="s">The content string.</param>
        /// <param name="value">The parsed result.</param>
        /// <returns>Whether the conversion succeeded.</returns>
        public static bool TryParseAsTimeSpan(this string s, out TimeSpan? value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return true;
            if (!s.StartsWith('+') && !s.StartsWith('-')) return false;
            var ts = s.Substring(1).Split(':', 3, StringSplitOptions.None);
            if (ts.Length != 3) return false;
            if (!int.TryParse(ts[0], out int hour)) return false;
            if (hour < 0) return false;
            if (!int.TryParse(ts[1], out int minutes)) return false;
            if (minutes < 0 || minutes >= 60) return false;
            if (!int.TryParse(ts[2], out int secs)) return false;
            if (secs < 0 || secs >= 60) return false;
            value = new TimeSpan(hour, minutes, secs);
            if (s.StartsWith('-')) value = -value;
            return true;
        }

        /// <summary>
        /// Convert the <see cref="TimeSpan"/> as relative time delta.
        /// </summary>
        /// <param name="timeSpan">The <see cref="TimeSpan"/>.</param>
        /// <returns>The result delta string.</returns>
        public static string ToDeltaString(this TimeSpan timeSpan)
        {
            char abs = timeSpan.TotalMilliseconds < 0 ? '-' : '+';
            timeSpan = timeSpan.Duration();
            return $"{abs}{Math.Floor(timeSpan.TotalHours)}:{timeSpan.Minutes:d2}:{timeSpan.Seconds:d2}";
        }
    }
}
