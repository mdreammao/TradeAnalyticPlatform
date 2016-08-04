using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    class TradeDaysUtils
    {
        private static List<DateTime> _tradeDays;
        private static List<DateTime> getTradeDays()
        {
            if (_tradeDays == null)
            {
                _tradeDays = (List<DateTime>)Platforms.basicInfo["TradeDays"];
            }
            return _tradeDays;
        }
        private static DateTime getTradeDay(int index)
        {
            if (index >= 0 && index < getTradeDays().Count)
            {
                return getTradeDays()[index];
            }
            return DateTime.MinValue; ;
        }

        /// <summary>
        /// 将DateTime格式的日期转化成为int类型的日期。
        /// </summary>
        /// <param name="time">DateTime类型的日期</param>
        /// <returns>Int类型的日期</returns>
        public static int DateTimeToInt(DateTime time)
        {
            return time.Year * 10000 + time.Month * 100 + time.Day;
        }

        /// <summary>
        /// 将Int格式的日期转化为DateTime格式类型的日期。
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static DateTime IntToDateTime(int day)
        {
            string dayString = DateTime.ParseExact(day.ToString(), "yyyyMMdd", null).ToString();
            return Convert.ToDateTime(dayString);
        }

        /// <summary>
        /// 静态函数。将数组下标转化为具体时刻。
        /// </summary>
        /// <param name="Index">下标</param>
        /// <returns>时刻</returns>
        public static int IndexToTime(int index)
        {
            int time0 = index * 500;
            int hour = time0 / 3600000;
            time0 = time0 % 3600000;
            int minute = time0 / 60000;
            time0 = time0 % 60000;
            int second = time0;
            if (hour < 2)
            {
                hour += 9;
                minute += 30;
                if (minute >= 60)
                {
                    minute -= 60;
                    hour += 1;
                }
            }
            else
            {
                hour += 11;
            }
            return hour * 10000000 + minute * 100000 + second;
        }


        /// <summary>
        /// 静态函数。将时间转化为数组下标。
        /// </summary>
        /// <param name="time">时间</param>
        /// <returns>数组下标</returns>
        public static int TimeToIndex(int time)
        {
            int hour = time / 10000000;
            time = time % 10000000;
            int minute = time / 100000;
            time = time % 100000;
            int tick = time / 500;
            int index;
            if (hour >= 13)
            {
                index = 14401 + (hour - 13) * 7200 + minute * 120 + tick;
            }
            else
            {
                index = (int)(((double)hour - 9.5) * 7200) + minute * 120 + tick;
            }
            return index;
        }

        /// <summary>
        /// 给出前一交易日。
        /// </summary>
        /// <param name="today">当前交易日</param>
        /// <returns>返回前一交易日</returns>
        public static DateTime PreviousTradeDay(DateTime today)
        {
            int index = getTradeDays().BinarySearch(today);
            return getTradeDay(index - 1);
        }

        /// <summary>
        /// 给出下一交易日。
        /// </summary>
        /// <param name="today">当前交易日</param>
        /// <returns>下一交易日</returns>
        public static DateTime NextTradeDay(DateTime today)
        {
            int index = getTradeDays().BinarySearch(today);
            return getTradeDay(index + 1);
        }

        /// <summary>
        /// 给出当前日期最近的交易日。如果今日是交易日返回今日，否者返回下一个最近的交易日。
        /// </summary>
        /// <param name="today">当前日期</param>
        /// <returns>交易日</returns>
        public static DateTime RecentTradeDay(DateTime today)
        {
            int index = getTradeDays().BinarySearch(today);
            return (index < 0) ? getTradeDay(-index) : today;
        }


        /// <summary>
        ///获取间隔的交易日计数,计数包含day1,day2。
        /// 例如，Jan-1,Jan-2,Jan-3不是交易日，Jan-4，Jan-5是交易日,则
        /// GetSpanOfTradeDays(Jan-4,Jan-5)=2
        /// GetSpanOfTradeDays(Jan-3,Jan-5)=2
        /// GetSpanOfTradeDays(Jan-1,Jan-3)=0
        /// </summary>
        /// <param name="day1">开始日期</param>
        /// <param name="day2">结束日期</param>
        /// <returns>间隔的交易日天数</returns>
        public static int GetSpanOfTradeDays(DateTime day1, DateTime day2)
        {
            int x1 = getTradeDays().BinarySearch(day1);
            int x2 = getTradeDays().BinarySearch(day2);
            if (x1 < 0) x1 = -x1;
            if (x2 < 0) x2 = -x2 - 1;
            return x2 - x1 + 1;
        }



        /// <summary>
        /// 判断当日是本月第几周。
        /// </summary>
        /// <param name="day">日期</param>
        /// <returns>第几周</returns>
        public static int WeekOfMonth(int day)
        {
            DateTime today = IntToDateTime(day);
            int daysOfWeek = 7;
            if (today.AddDays(0 - daysOfWeek).Month != today.Month) return 1;
            if (today.AddDays(0 - 2 * daysOfWeek).Month != today.Month) return 2;
            if (today.AddDays(0 - 3 * daysOfWeek).Month != today.Month) return 3;
            if (today.AddDays(0 - 4 * daysOfWeek).Month != today.Month) return 4;
            return 5;
        }


    }
}
