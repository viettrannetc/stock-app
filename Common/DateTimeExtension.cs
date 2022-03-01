using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace DotNetCoreSqlDb.Common
{
    public static class DateTimeExtension
    {
        /// <summary>
        /// https://stackoverflow.com/questions/8454974/c-sharp-net-equivalent-to-php-time
        /// </summary>
        public static int ConvertToPhpInt(this DateTime dateTime)
        {
            var result = (int)(dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
            return result;
        }

        /// <summary>
        /// https://stackoverflow.com/questions/25399600/parsing-integer-value-as-datetime
        /// </summary>
        public static DateTime PhpIntConvertToDateTime(this int phpDateTimeNumber)
        {
            var result = new DateTime(1970, 1, 1).AddSeconds(phpDateTimeNumber);
            return result;
        }

        public static bool IsWeekend(this DateTime today)
        {
            return (today.DayOfWeek == DayOfWeek.Saturday) || (today.DayOfWeek == DayOfWeek.Sunday);
        }

        public static DateTime WithoutHours(this DateTime today)
        {
            return new DateTime(today.Year, today.Month, today.Day, 0, 0, 0);
        }

        public static T DeepClone<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        // This presumes that weeks start with Monday.
        // Week 1 is the 1st week of the year with a Thursday in it.
        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
