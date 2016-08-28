using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.TimeList
{
    public static class TimeListUtility
    {

        /// <summary>
        /// 将数组下标转化为DateTime格式的时间
        /// </summary>
        /// <param name="today">今日日期(yyyyMMdd格式)</param>
        /// <param name="index">数组下标(0-239)</param>
        /// <returns></returns>
        public static DateTime IndexToMinuteDateTime(int today,int index)
        {
            index = index + 1;
            DateTime time = Kit.ToDate(today);
            if (index<=1)
            {
                index = 1;
            }
            if (index>=240)
            {
                index = 240;
            }
            if (index<=120)
            {
                return time.AddMinutes(index - 1 + 570);
            }
            else
            {
                return time.AddMinutes(index - 1 + 570 + 90);
            }
            return time;    
        }
        
        /// <summary>
        /// 将时间变为数组下标。一天对应240个分钟,分别对应0到239。
        /// </summary>
        /// <param name="time">DateTime格式的时间</param>
        /// <returns>数组下标</returns>
        public static int MinuteToIndex(DateTime time)
        {
            int hour = time.Hour;
            int minute = time.Minute;
            if (hour<13)
            {
                return (((hour - 9) * 60 + (minute - 30) + 1)<0?0 :(hour - 9) * 60 + (minute - 30) + 1)-1;
            }
            else
            {
                return (((hour - 13) * 60+ minute + 121)>240?240: (hour - 13) * 60 + minute + 121)-1;
            }
        }

        /// <summary>
        /// 将时间变为数组下标。一天对应28802个tick数据，包含上午收盘和下午收盘的价格
        /// </summary>
        /// <param name="time">时间(int)</param>
        /// <returns></returns>
        public static int TickToIndex(int time)
        {
            int hour = time / 10000000;
            time = time % 10000000;
            int minute = time / 100000;
            time = time % 100000;
            int tick = time / 500;
            int index=0;
            if ((time>=93000000 && time<=113000000) ||(time>=130000000 && time<=150000000))
            {
                if (hour >= 13)
                {
                    index = 14401 + (hour - 13) * 7200 + minute * 120 + tick;
                }
                else
                {
                    index = (int)(((double)hour - 9.5) * 7200) + minute * 120 + tick;
                }
            }
            else if (time > 113000000 && time < 113100000)
            {
                index = 14401;
            }
            else if (time>150000000 && time<150100000)
            {
                index = 28802;
            }
            return index;
        }

        /// <summary>
        /// 将时间变为数组下标。一天对应28802个tick数据，包含上午收盘和下午收盘的价格
        /// </summary>
        /// <param name="time">时间(DateTime)</param>
        /// <returns></returns>
        public static int TickToIndex(DateTime time)
        {

            int t = time.Hour * 10000000 + time.Minute * 100000 + time.Second * 1000 + time.Millisecond;
            return TickToIndex(t);
        }

    }
}
