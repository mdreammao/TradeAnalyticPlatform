using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.TimeList
{
    public static class CreateTimeList
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        public static List<DateTime> GenerateTickTimeList(params DateTime[] timeList)
        {
            List<DateTime> time = new List<DateTime>();
            if (timeList==null || timeList.Count()==0 || timeList.Count()%2==1)
            {
                log.Error("Wrong timeList input!");
                return null;
            }
            for (int i = 0; i < timeList.Count(); i=i+2)
            {
                time.AddRange(GenerateEachTickTimeList(timeList[i], timeList[i + 1]));
            }
            return time;
        }

        private static List<DateTime> GenerateEachTickTimeList(DateTime start,DateTime end)
        {
            List<DateTime> time = new List<DateTime>();
            if (end<=start)
            {
                log.Error("Wrong timeList input!");
                return null;
            }
            DateTime now = start;
            while (now<=end)
            {
                time.Add(now);
                now=now.AddMilliseconds(500);
            }
            return time;
        }
        public static List<DateTime> GenerateMinuteTimeList(params DateTime[] timeList)
        {
            List<DateTime> time = new List<DateTime>();
            if (timeList == null || timeList.Count() == 0 || timeList.Count() % 2 == 1)
            {
                log.Error("Wrong timeList input!");
                return null;
            }
            for (int i = 0; i < timeList.Count(); i = i + 2)
            {
                time.AddRange(GenerateEachMinuteTimeList(timeList[i], timeList[i + 1]));
            }
            return time;
        }

        private static List<DateTime> GenerateEachMinuteTimeList(DateTime start, DateTime end)
        {
            List<DateTime> time = new List<DateTime>();
            if (end <= start)
            {
                log.Error("Wrong timeList input!");
                return null;
            }
            DateTime now = start;
            while (now <= end)
            {
                time.Add(now);
                now = now.AddMinutes(1);
            }
            return time;
        }
    }
}
