using System;

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
    }
}
