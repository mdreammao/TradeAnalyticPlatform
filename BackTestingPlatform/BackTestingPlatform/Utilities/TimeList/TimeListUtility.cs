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
        /// 将时间变为数组下标。一天对应28802个tick数据，包含上午收盘和下午收盘的价格
        /// </summary>
        /// <param name="time">时间(int)</param>
        /// <returns></returns>
        public static int ToTickIndex(int time)
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
        public static int ToTickIndex(DateTime time)
        {

            int t = time.Hour * 10000000 + time.Minute * 100000 + time.Second * 1000 + time.Millisecond;
            return ToTickIndex(t);
        }

    }
}
