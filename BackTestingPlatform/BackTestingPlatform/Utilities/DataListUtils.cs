using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model.Common;
using System.Collections;


namespace BackTestingPlatform.Utilities
{
    /// <summary>
    /// List按时间区间部分复制，输入原List、起始时间及结束时间，返回子Lsit
    /// </summary>
    [Obsolete]
    public static class DataListUtils
    {

        public static List<TickFromMssql> ModifyListByDate(List<TickFromMssql> originalList, int startDate, int endDate)
        {
            int startIndex = originalList.FindIndex(s => s.date >= startDate);
            int endIndex = originalList.FindIndex(s => s.date >= endDate);
            if (startIndex != -1 & endIndex != -1)
                return originalList.GetRange(startIndex, endIndex - startIndex + 1);
            else
                return null;
        }


        public static List<TickFromMssql> ModifyListByTime(List<TickFromMssql> originalList, int startTime, int endTime)
        {
            int startIndex = originalList.FindIndex(s => s.time >= startTime);
            int endIndex = originalList.FindIndex(s => s.time >= endTime);
            if (startIndex != -1 & endIndex != -1)
                return originalList.GetRange(startIndex, endIndex - startIndex + 1);
            else
                return null;
        }

        public static List<TickFromMssql> FillList(List<TickFromMssql> originalList)
        {
            TickFromMssql[] arr = new TickFromMssql[14402];
            foreach (var item in originalList)
            {
                int index = TimeToIndex(item.time);
                if (index >= 0 && index <= 14401)
                {
                    arr[index] = item;
                }
            }
            for (int i = 1; i < 14402; i++)
            {
                TickFromMssql thisTick = arr[i];
                if (thisTick == null)
                {
                    arr[i] = arr[i - 1];
                }
            }
            return arr.ToList();
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


}

