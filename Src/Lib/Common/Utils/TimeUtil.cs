using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    public class TimeUtil
    {
        public static double timestamp
        {
            get { return GetTimestamp(DateTime.Now); }
        }

        public static DateTime GetTime(long timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = timeStamp * 10000000;
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        public static double GetTimestamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (time - startTime).TotalSeconds;
        }
    }
}
