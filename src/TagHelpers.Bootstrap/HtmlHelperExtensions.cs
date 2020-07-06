using System;
using System.Globalization;

namespace Microsoft.AspNetCore.Mvc.Rendering
{
    public static class HtmlHelperExtensions
    {
        private static readonly CultureInfo EnglishCulture
            = CultureInfo.GetCultureInfo(1033);

        /// <summary>
        /// Return auto unit time span display, such as <c>23 months</c>.
        /// </summary>
        /// <param name="_">The html helper</param>
        /// <param name="timeSpan">The timespan to format</param>
        /// <returns>auto unit timespan</returns>
        public static string AutoUnitTimespan(this IHtmlHelper _, TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 730) return $"{timeSpan.TotalDays / 365:0} years";
            else if (timeSpan.TotalDays > 60) return $"{timeSpan.TotalDays / 30:0} months";
            else if (timeSpan.TotalDays > 14) return $"{timeSpan.TotalDays / 7:0} weeks";
            else if (timeSpan.TotalDays > 2) return $"{timeSpan.TotalDays:0} days";
            else if (timeSpan.TotalHours > 2) return $"{timeSpan.TotalHours:0} hours";
            else if (timeSpan.TotalMinutes > 2) return $"{timeSpan.TotalMinutes:0} mins";
            return $"{timeSpan.TotalSeconds:0} secs";
        }

        /// <summary>
        /// Return a formatted CST Time display.
        /// </summary>
        /// <param name="_">The html helper</param>
        /// <param name="_dt">The datetime with zone offset</param>
        /// <param name="cnStyle">Show as CN style</param>
        /// <returns>
        /// If <paramref name="cnStyle"/> is set to true, return the "2020/6/16 00:00:00". <br/>
        /// Otherwise, returns the "Tue, 16 Jun 2020 00:00:00 CST" if zone is UTC+8, or GMT with converted to UTC.
        /// </returns>
        public static string CstTime(this IHtmlHelper _, DateTimeOffset? _dt, bool cnStyle = true)
        {
            if (!_dt.HasValue) return "";
            var dt = _dt.Value;

            if (cnStyle) dt.ToString("yyyy/MM/dd HH:mm:ss");
            if (dt.Offset == TimeSpan.FromHours(8))
                return dt.ToString("ddd, dd MMM yyyy HH':'mm':'ss 'CST'", EnglishCulture);
            else
                return dt.ToUniversalTime().ToString("R", EnglishCulture);
        }

        /// <summary>
        /// Return a formatted timespan display.
        /// </summary>
        /// <param name="_">The html helper</param>
        /// <param name="timespan">The timespan to format</param>
        /// <returns><c>-</c> if <c>null</c>, otherwise <c>233:00</c></returns>
        public static string MinSecTimespan(this IHtmlHelper _, TimeSpan? timespan)
        {
            if (!timespan.HasValue) return "-";
            int tot = (int)timespan.Value.TotalSeconds;
            return $"{tot / 60:00}:{tot % 60:00}";
        }

        /// <summary>
        /// Return a formatted ratio display.
        /// </summary>
        /// <param name="_">The html helper</param>
        /// <param name="timespan">The timespan to format</param>
        /// <returns><c>-</c> if <c>null</c>, otherwise <c>233:00</c></returns>
        public static string RatioOf(this IHtmlHelper _, int fz, int fm)
        {
            return fm == 0 ? "0.00% (0/0)" : $"{100.0 * fz / fm:F2}% ({fz}/{fm})";
        }

        /// <summary>
        /// Return a formatted file size display.
        /// </summary>
        /// <param name="_">The html helper</param>
        /// <param name="size">The file size</param>
        /// <returns><c>2.11M</c>, <c>9.85K</c> or <c>12B</c></returns>
        public static string AutoUnitFileSize(this IHtmlHelper _, long size)
        {
            if (size > 1048576) return $"{size / 1048576.0:F2}M";
            else if (size > 1024) return $"{size / 1024.0:F2}K";
            return $"{size}B";
        }
    }
}
