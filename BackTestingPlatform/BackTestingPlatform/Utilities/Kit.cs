using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    /// <summary>
    /// 常用且较为通用的工具类，大多数是极其常用的类型转换。
    /// 包括一些类型安全的转换，包容null值，溢出等异常情况。
    /// </summary>
    public static class Kit
    {

        /// <summary>
        /// 转换到DateTime类型，遇到非法转换则返回DateTime.MinValue
        /// 用例：
        /// ToDateTime(20160805,93000)
        /// ToDateTime(20160805,93000000)
        /// </summary>
        /// <param name="tdate"></param>
        /// <param name="ttime"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(int tdate, int ttime, bool considerMillis = true)
        {
            int y = tdate / 10000, m = (tdate % 10000) / 100, d = tdate % 100;
            if (y < 1 || m == 0 || d == 0)
                return DateTime.MinValue;
            if (considerMillis && ttime > 240000)
                ttime = ttime / 1000;   //ttime可能包含了毫秒值
            if (ttime < 0 || ttime > 240000)
                return DateTime.MinValue;

            return new DateTime(y, m, d,
                 ttime / 10000, (ttime % 10000) / 100, ttime % 100);
        }
        public static DateTime ToDateTime(string tdate, string ttime)
        {
            int d = 0, t = 0;
            int.TryParse(tdate, out d);
            int.TryParse(ttime, out t);
            return ToDateTime(d, t);
        }
        /// <summary>
        /// 转换到DateTime类型，遇到非法转换则返回DateTime.MinValue，用例：
        /// ToDateTime(20160805)
        /// ToDateTime(20160805093000)
        /// ToDateTime(20160805093000000)
        /// </summary>
        /// <param name="arg">类似20160805093000</param>
        /// <returns></returns>
        public static DateTime ToDateTime(long arg)
        {
            if (arg >= 10000000000000000) arg = arg / 1000; //可能包含了毫秒值
            if (arg < 100000000) arg = arg * 100000000; //可能未包含hhmmss
            return ToDateTime((int)(arg / 1000000), (int)(arg % 1000000));
        }
        /// <summary>
        /// 转换到DateTime类型,用例：
        /// ToDateTime(20160805093000)
        /// </summary>
        /// <param name="arg">类似20160805093000</param>
        /// <returns></returns>
        public static DateTime ToDateTime(int arg)
        {
            return ToDateTime(arg / 1000000, arg % 1000000, false);
        }

        /// <summary>
        /// 转换到DateTime类型,用例：
        /// ToDateTime(20160805093000)
        /// </summary>
        /// <param name="arg">类似yyyyMMddhhmmss</param>
        /// <returns></returns>
        public static DateTime ToDateTime(string arg)
        {
            long x = 0;
            long.TryParse(arg, out x);
            return ToDateTime(x);
        }
        /// <summary>
        /// 转换到DateTime类型,不含hhmmss,用例：
        /// ToDateTime(20160805),相当于ToDateTime(20160805000000)
        /// </summary>
        /// <param name="arg">类似yyyyMMdd</param>
        /// <returns></returns>
        public static DateTime ToDate(int arg)
        {
            return ToDateTime(arg, 0);
        }
        /// <summary>
        /// 返回值类似20160805093000
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int ToInt_yyyyMMddHHmmss(DateTime t)
        {
            return (t.Year * 10000 + t.Month * 100 + t.Day) * 1000000
            + t.Hour * 10000 + t.Minute * 100 + t.Second;
        }

        /// <summary>
        /// 返回值类似2016080509
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static int ToInt_yyyyMMdd(DateTime t)
        {
            return (t.Year * 10000 + t.Month * 100 + t.Day);
        }

        /// <summary>
        /// 安全的类型转换。
        /// 相较于Convert.ToInt32(object)更为安全。
        /// 若发生溢出，null值等无法转换的情形，返回0
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static int ToInt(object arg)
        {
            if (arg == null)
                return 0;
            if (arg.GetType() == typeof(decimal))
                return (int)arg;
            if (arg.GetType() == typeof(int))
                return (int)arg;
            if (arg.GetType() == typeof(double))
                return (int)arg;
            if (arg.GetType() == typeof(long))
                return (int)arg;
            if (arg.GetType() == typeof(string))
            {
                int r = 0;
                int.TryParse((string)arg, out r);
                return r;
            }
            return 0;
        }
        /// <summary>
        /// 安全的类型转换。
        /// 相较于Convert.ToDouble(object)更为安全。
        /// 若发生溢出，null值等无法转换的情形，返回0
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static double ToDouble(object arg)
        {
            if (arg == null)
                return 0;
            if (arg.GetType() == typeof(decimal))
                return (double)arg;
            if (arg.GetType() == typeof(int))
                return (double)arg;
            if (arg.GetType() == typeof(double))
                return (double)arg;
            if (arg.GetType() == typeof(long))
                return (double)arg;
            if (arg.GetType() == typeof(string))
            {
                double r = 0;
                double.TryParse((string)arg, out r);
                return r;
            }
            return 0;
        }

    }
}
