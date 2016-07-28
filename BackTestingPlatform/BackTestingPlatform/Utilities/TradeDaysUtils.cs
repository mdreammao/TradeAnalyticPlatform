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
        /// 静态函数。给出下一交易日。
        /// </summary>
        /// <param name="today">当前交易日</param>
        /// <returns>下一交易日</returns>
        public static int GetNextTradeDay(int today)
        {
            int nextIndex = Platforms.tradeDays.FindIndex(delegate (int i) { return i == today; }) + 1;
            if (nextIndex >= Platforms.tradeDays.Count)
            {
                return 0;
            }
            else
            {
                return Platforms.tradeDays[nextIndex];
            }
        }

        /// <summary>
        /// 给出当前日期最近的交易日。如果今日是交易日返回今日，否者返回下一个最近的交易日。
        /// </summary>
        /// <param name="today">当前日期</param>
        /// <returns>交易日</returns>
        public static int GetRecentTradeDay(int today)
        {

            for (int i = 0; i < Platforms.tradeDays.Count - 1; i++)
            {
                if (Platforms.tradeDays[i] == today)
                {
                    return today;
                }
                if (Platforms.tradeDays[i] < today && Platforms.tradeDays[i + 1] >= today)
                {
                    return Platforms.tradeDays[i + 1];
                }
            }
            return 0;
        }

        /// <summary>
        /// 静态函数。给出前一交易日。
        /// </summary>
        /// <param name="today">当前交易日</param>
        /// <returns>返回前一交易日</returns>
        public static int GetPreviousTradeDay(int today)
        {
            int preIndex = Platforms.tradeDays.FindIndex(delegate (int i) { return i == today; }) - 1;
            if (preIndex < 0)
            {
                return 0;
            }
            else
            {
                return Platforms.tradeDays[preIndex];
            }
        }


        /// <summary>
        /// 静态函数。获取交易日间隔天数。
        /// </summary>
        /// <param name="firstday">开始日期</param>
        /// <param name="lastday">结束日期</param>
        /// <returns>间隔天数</returns>
        public static int GetTimeSpan(int firstday, int lastday)
        {
            if (firstday >= Platforms.tradeDays[0] && lastday <= Platforms.tradeDays[Platforms.tradeDays.Count - 1] && lastday >= firstday)
            {
                int startIndex = -1, endIndex = -1;
                for (int i = 0; i < Platforms.tradeDays.Count; i++)
                {
                    if (Platforms.tradeDays[i] == firstday)
                    {
                        startIndex = i;
                    }
                    if (Platforms.tradeDays[i] > firstday && Platforms.tradeDays[i - 1] < firstday)
                    {
                        startIndex = i;
                    }
                    if (Platforms.tradeDays[i] == lastday)
                    {
                        endIndex = i;
                    }
                    if (Platforms.tradeDays[i] > lastday && Platforms.tradeDays[i - 1] < lastday)
                    {
                        endIndex = i - 1;
                    }
                }
                if (startIndex != -1 && endIndex != -1)
                {
                    return endIndex - startIndex + 1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
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
