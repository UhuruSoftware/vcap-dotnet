using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Uhuru.Utilities
{
    /// <summary>
    /// This is a helper class that containg methods useful for converting .Net variables to Ruby formats.
    /// </summary>
    public static class RubyCompatibility
    {
        /// <summary>
        /// This method converts a DateTime to its equivalent in number of seconds from the Epoch - 1st of January 1970.
        /// </summary>
        /// <param name="date">DateTime to be converted.</param>
        /// <returns>An int containing the number of seconds.</returns>
        public static int DateTimeToEpochSeconds(DateTime date)
        {
            return (int)(date - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        /// <summary>
        /// This method converts a number of seconds from the Epoch (1st of January 1970) to a DateTime value.
        /// </summary>
        /// <param name="seconds">An int containing the number of seconds.</param>
        /// <returns>A DateTime containing the converted value.</returns>
        public static DateTime DateTimeFromEpochSeconds(int seconds)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0) + new TimeSpan(0, 0, seconds);
        }

        /// <summary>
        /// This method converts a DateTime value to a Ruby Time string (yyyy-MM-dd HH:mm:ss zzz).
        /// </summary>
        /// <param name="date">The DateTime to be converted.</param>
        /// <returns>A string with the formatted date and time.</returns>
        public static string DateTimeToRubyString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// This method converts a Ruby Time string (yyyy-MM-dd HH:mm:ss zzz) to a .Net DateTime.
        /// </summary>
        /// <param name="date">A string containing the formatted date and time.</param>
        /// <returns>A DateTime containing the converted value.</returns>
        public static DateTime DateTimeFromRubyString(string date)
        {
            DateTimeFormatInfo dateFormat = new DateTimeFormatInfo();
            dateFormat.SetAllDateTimePatterns(new string[] { "yyyy-MM-dd HH:mm:ss zzz" }, 'Y');
            return DateTime.Parse(date, dateFormat);
        }
    }
}
