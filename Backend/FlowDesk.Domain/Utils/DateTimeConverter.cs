using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Domain.Utils
{
    public static class DateTimeConverter
    {
        private static readonly TimeZoneInfo IstZone =
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public static DateTime ToIst(DateTime dateTime)
        {
            if (dateTime == default) return dateTime;

            // Treat Unspecified as UTC (common with EF + SQL)
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            return dateTime.Kind == DateTimeKind.Utc
                ? TimeZoneInfo.ConvertTimeFromUtc(dateTime, IstZone)
                : dateTime;
        }
    }
}
