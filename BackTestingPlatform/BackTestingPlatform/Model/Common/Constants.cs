using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{
    public static class Constants
    {

        public static TimeLine timeline500ms = new TimeLine(
                new TimeLineSection("09:30:00.000", "11:30:00.000", 500),
                new TimeLineSection("13:00:00.000", "15:00:00.000", 500)
                );

        public static int INF = 99999, NINF = -99999, NONE = -88888;
        public static DateTime TRADE_DAY_START = new DateTime(2007, 1, 1, 0, 0, 0);
        public static DateTime TRADE_DAY_END = new DateTime(2016, 12, 31, 23, 59, 59);
    }
}
