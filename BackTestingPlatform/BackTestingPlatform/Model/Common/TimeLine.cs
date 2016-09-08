using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{

    public abstract class TimeLine
    {
        public abstract List<int> toListOfMillis();
    }

    public class StandardTimeLine : TimeLine
    {
        IList<TimeLineSection> sections;

        public override List<int> toListOfMillis()
        {
            List<int> res = new List<int>();
            int i, k = -1;
            for (i = 0; i < sections.Count; i++)
            {
                var sect = sections[i];
                if (k == sect.start) k += sect.interval;
                for (k = sect.start; k <= sect.end; k += sect.interval)
                {
                    res.Add(k);
                }
            }
            return res;
        }
        public StandardTimeLine(IList<TimeLineSection> sections)
        {
            this.sections = sections;
        }
    }

    public class SimpleTimeLine : TimeLine
    {
        IList<int> millis;
        public override List<int> toListOfMillis()
        {
            return millis.ToList();
        }
        public SimpleTimeLine(IList<int> millis)
        {
            this.millis = millis;
        }

        public SimpleTimeLine(IList<DateTime> times)
        {
            millis = times.Select(t => (int)t.TimeOfDay.TotalMilliseconds).ToList();
        }

    }

    public struct TimeLineSection
    {
        public int start, end, interval; //millisecond values

        public TimeLineSection(string TimeStart, string timeEnd, int millisInterval)
        {
            start = ToIntOfMillis(TimeStart);
            end = ToIntOfMillis(timeEnd);
            interval = millisInterval;
        }
        public List<int> ToListOfMillis()
        {
            var res = new List<int>((end - start) / interval + 1);
            for (int k = start; k <= end; k += interval)
            {
                res.Add(k);
            }
            return res;
        }

        // "09:30:15.400" -> 1945815400
        static int ToIntOfMillis(string src)
        {
            DateTime dt = DateTime.ParseExact(src, "HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            return (int)dt.TimeOfDay.TotalMilliseconds;
        }

    }

    public static class TimeLines
    {
       
    }
}
