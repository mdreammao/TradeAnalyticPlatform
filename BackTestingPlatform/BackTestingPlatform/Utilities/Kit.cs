using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    /// <summary>
    /// 常用工具函数，包含常用的类型转换
    /// </summary>
    public static class Kit
    {

        /// <summary>
        /// 转换到DateTime类型，遇到非法转换则返回DateTime.MinValue
        /// 用例：
        /// toDateTime(20160805,93000)
        /// toDateTime(20160805,93000000)
        /// </summary>
        /// <param name="tdate"></param>
        /// <param name="ttime"></param>
        /// <returns></returns>
        public static DateTime toDateTime(int tdate, int ttime, bool considerMillis = true)
        {
            int y = tdate / 10000, m = (tdate % 10000) / 100, d = tdate % 100;
            if (y < 0 || m == 0 || d == 0)
                return DateTime.MinValue;
            if (considerMillis && ttime > 240000)
                ttime = ttime / 1000;   //ttime可能包含了毫秒值
            if (ttime < 0 || ttime > 240000)
                return DateTime.MinValue;
            
            return new DateTime(y,m,d,
                 ttime / 10000, (ttime % 10000) / 100, ttime % 100);
        }
        public static DateTime toDateTime(string tdate, string ttime)
        {
            int d = 0, t = 0;
            int.TryParse(tdate, out d);
            int.TryParse(ttime, out t);
            return toDateTime(d, t);
        }
        /// <summary>
        /// 转换到DateTime类型，遇到非法转换则返回DateTime.MinValue，用例：
        /// toDateTime(20160805)
        /// toDateTime(20160805093000)
        /// toDateTime(20160805093000000)
        /// </summary>
        /// <param name="arg">类似20160805093000</param>
        /// <returns></returns>
        public static DateTime toDateTime(long arg)
        {
            if (arg >= 100000000000000) arg = arg / 1000; //可能包含了毫秒值
            if (arg < 100000000) arg = arg * 100000000; //可能未包含hhmmss
            return toDateTime((int)(arg / 1000000), (int)(arg % 1000000));
        }
        /// <summary>
        /// 转换到DateTime类型,用例：
        /// toDateTime(20160805093000)
        /// </summary>
        /// <param name="arg">类似20160805093000</param>
        /// <returns></returns>
        public static DateTime toDateTime(int arg)
        {
            return toDateTime(arg / 1000000, arg % 1000000, false);
        }

        /// <summary>
        /// 转换到DateTime类型,用例：
        /// toDateTime(20160805093000)
        /// </summary>
        /// <param name="arg">类似yyyyMMddhhmmss</param>
        /// <returns></returns>
        public static DateTime toDateTime(string arg)
        {
            long x = 0;
            long.TryParse(arg, out x);
            return toDateTime(x);
        }
        /// <summary>
        /// 转换到DateTime类型,不含hhmmss,用例：
        /// toDateTime(20160805),相当于toDateTime(20160805000000)
        /// </summary>
        /// <param name="arg">类似yyyyMMdd</param>
        /// <returns></returns>
        public static DateTime toDate(int arg)
        {
            return toDateTime(arg, 0);
        }
        /// <summary>
        /// 返回值类似20160805093000
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int to14digitsInt(DateTime t)
        {
            return (t.Year * 10000 + t.Month * 100 + t.Day) * 1000000
            + t.Hour * 10000 + t.Minute * 100 + t.Second;
        }
        /// <summary>
        /// 返回值类似"20160805093000"
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int to14digitsString(DateTime t)
        {
            return (t.Year * 10000 + t.Month * 100 + t.Day) * 1000000
            + t.Hour * 10000 + t.Minute * 100 + t.Second;
        }


        public static string to_yyyyMMdd(DateTime t)
        {
            return t.ToString("yyyyMMdd");
        }

        public static int toDateInt(DateTime t)
        {
            return Convert.ToInt32(t.ToString("yyyyMMdd"));
        }

    }
}
