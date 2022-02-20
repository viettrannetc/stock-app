using System;
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
    }
}
