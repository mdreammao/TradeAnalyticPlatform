using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.Common
{
    /// <summary>
    /// 时间序列相关的工具类
    /// </summary>
    public static class SequentialUtils
    {

        /// <summary>
        /// 在已按时间递增排序好的list中二分查找指定时间的元素下标。
        /// 若找到返回匹配元素的下标，若没有返回不小于指定时间的元素最大下标值的相反数.
        /// 该实现应该比包装原生List.BinarySearch要快些
        /// </summary>
        /// <typeparam name="T">元素类型，必须继承了Sequential</typeparam>
        /// <param name="ascList">数据列表，确保已经按时间递增排序好</param>
        /// <param name="targetTime">指定时间</param>
        /// <returns>若找到返回匹配元素的下标，若没有返回不小于指定时间的元素最小下标值的补</returns>
        public static int BinarySearch<T>(List<T> ascList, DateTime targetTime) where T : Sequential
        {
            int lo = 0;
            int hi = ascList.Count - 1;
            int mid;
            while (lo <= hi)
            {
                mid = lo + (hi - lo) / 2;
                var tt = ascList[mid].time;
                if (ascList[mid].time > targetTime)
                {
                    hi = mid - 1;
                }
                else if (ascList[mid].time < targetTime)
                {
                    lo = mid + 1;
                }
                else
                {
                    return mid;
                }
            }
            return ~lo;
        }

        /// <summary>
        /// 返回一个新的list，是原来的按时间递增有序list的子集,即：
        /// timeEnd >= 新list任何元素 >= timeStart
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="timeStart">起始时间，包含本身</param>
        /// <param name="timeEnd">结束时间，包含本身</param>
        /// <returns></returns>
        public static List<T> GetRange<T>(List<T> src, DateTime timeStart, DateTime timeEnd) where T : Sequential
        {
            int x1 = BinarySearch(src, timeStart);
            int x2 = BinarySearch(src, timeEnd);
            x1 = x1 < 0 ? ~x1 : x1;
            x2 = x2 < 0 ? ~x2 - 1 : x2;
            return src.GetRange(x1, x2 - x1 + 1);
        }

        /// <summary>
        /// 是否是按时间递增有序的
        /// </summary>
        /// <returns></returns>
        public static bool IsOrderedAsc<T>(IList<T> src) where T : Sequential
        {
            for (int i = 0; i < src.Count - 1; i++)
            {
                if (src[i].time > src[i + 1].time) return false;
            }
            return true;
        }
    }
}
