using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    public class DateUtils
    {
        private static List<DateTime> _tradeDays;
        private static List<DateTime> getTradeDays()
        {
            if (_tradeDays == null)
            {
                _tradeDays = Caches.get<List<DateTime>>("TradeDays");
            }
            return _tradeDays;
        }
        private static DateTime getTradeDay(int index)
        {
            if (index >= 0 && index < getTradeDays().Count)
            {
                return getTradeDays()[index];
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// get TradeDay List by a range [firstDate,lastDate]
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="lastDate"></param>
        /// <returns></returns>
        public static List<DateTime> GetTradeDays(DateTime firstDate, DateTime lastDate)
        {
            int x1 = getTradeDays().BinarySearch(firstDate);
            int x2 = getTradeDays().BinarySearch(lastDate);
            x1 = x1 < 0 ? -x1 - 1 : x1;
            x2 = x2 < 0 ? -x2 - 2 : x2;
            return getTradeDays().GetRange(x1, x2 - x1 + 1);
        }

        public static List<DateTime> GetTradeDays(int firstDate, int lastDate)
        {
            return GetTradeDays(
                Kit.ToDateTime(firstDate, 0),
                Kit.ToDateTime(lastDate, 235959));
        }



        /// <summary>
        /// 判断当前日期是否为交易日
        /// </summary>
        /// <param name="today">当前日</param>
        /// <returns></returns>
        public static bool IsTradeDay(DateTime today)
        {
            return getTradeDays().BinarySearch(today) >= 0;
        }

        static int _IndexOfPreviousTradeDay(DateTime today)
        {
            int x = getTradeDays().BinarySearch(today);
            return x < 0 ? -x - 2 : x - 1;
        }
        static int _IndexOfNextTradeDay(DateTime today)
        {
            int x = getTradeDays().BinarySearch(today);
            return x < 0 ? -x - 1 : x + 1;
        }

        /// <summary>
        /// 给出上一交易日,即比当前天早的交易日中最晚的一个
        /// </summary>
        /// <param name="today">当前日</param>
        /// <returns>返回前一交易日</returns>
        public static DateTime PreviousTradeDay(DateTime today)
        {
            return getTradeDay(_IndexOfPreviousTradeDay(today));
        }

        /// <summary>
        /// 给出下一交易日,即比当前天晚的交易日中最早的一个
        /// </summary>
        /// <param name="today">当前日</param>
        /// <returns>下一交易日</returns>
        public static DateTime NextTradeDay(DateTime today)
        {
            return getTradeDay(_IndexOfNextTradeDay(today));
        }

        /// <summary>
        /// 给出离当前日期最近的交易日。如果今日是交易日返回今日，否者返回上一个最近的交易日。
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        public static DateTime PreviousOrCurrentTradeDay(DateTime today)
        {
            int x = getTradeDays().BinarySearch(today);
            x = x < 0 ? -x - 2 : x;
            return getTradeDay(x);
        }

        /// <summary>
        /// 给出离当前日期最近的交易日。如果今日是交易日返回今日，否者返回下一个最近的交易日。
        /// </summary>
        /// <param name="today">当前日期</param>
        /// <returns>交易日</returns>
        public static DateTime NextOrCurrentTradeDay(DateTime today)
        {
            int x = getTradeDays().BinarySearch(today);
            x = x < 0 ? -x - 1 : x;
            return getTradeDay(x);
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

            x1 = x1 < 0 ? -x1 - 1 : x1;
            x2 = x2 < 0 ? -x2 - 2 : x2;
            return x2 - x1 + 1;
        }



        /// <summary>
        /// 获取当日是本月第几周。
        /// </summary>
        /// <param name="day">日期</param>
        /// <returns>第几周</returns>
        public static int GetWeekOfMonth(DateTime date)
        {
            int weekNum = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return weekNum;
        }

        /// <summary>
        /// 获取第三个周五
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public static DateTime GetThirdFridayOfMonth(int year, int month)
        {
            var d = new DateTime(year, month, 15);  //3rd Friday must between day 15 ~ 21
            while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
            return d;
        }
        /// <summary>
        /// 获取第三个周五
        /// </summary>
        /// <param name="aDateOfThisMonth"></param>
        /// <returns></returns>
        public static DateTime GetThirdFridayOfMonth(DateTime aDateOfThisMonth)
        {
            return GetThirdFridayOfMonth(aDateOfThisMonth.Year, aDateOfThisMonth.Month);
        }
    }
}
