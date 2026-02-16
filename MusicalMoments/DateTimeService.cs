using System;
using System.Globalization;

namespace MusicalMoments
{
    internal static class DateTimeService
    {
        public static int CalculateDaysBetweenDates(string dateStr1, string dateStr2)
        {
            string[] formats =
            {
                "yyyy年MM月dd日HH时mm分ss秒",
                "yyyy年MM月dd日HH时mm分",
                "yyyy年MM月dd日HH时",
                "yyyy年MM月dd日",
                "yyyy年MM月dd",
                "yyyy年MM月",
                "yyyy年MM",
                "yyyy年"
            };

            string normalizedDate1 = dateStr1.Replace(" ", "");
            string normalizedDate2 = dateStr2.Replace(" ", "");

            if (!DateTime.TryParseExact(normalizedDate1, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date1))
            {
                throw new ArgumentException("Invalid date format for the first date.");
            }

            if (!DateTime.TryParseExact(normalizedDate2, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date2))
            {
                throw new ArgumentException("Invalid date format for the second date.");
            }

            return Math.Abs((date2 - date1).Days);
        }
    }
}
