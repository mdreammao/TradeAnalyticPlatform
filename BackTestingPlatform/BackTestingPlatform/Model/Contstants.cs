using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model
{
    /// <summary>
    /// 定义部分可能使用的常量
    /// </summary>
    public class Constants
    {
        public static int INF = 99999, NINF = -99999, NONE = -88888;

        public static DateTime TRADE_DAY_START = new DateTime(2007, 1, 1, 0, 0, 0);
        public static DateTime TRADE_DAY_END = new DateTime(2016, 12, 31, 23, 59, 59);
    }
   


}
