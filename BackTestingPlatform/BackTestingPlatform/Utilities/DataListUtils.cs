using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model.Common;
using System.Collections.Generic;
using System.Collections;


namespace BackTestingPlatform.Utilities
{
    /// <summary>
    /// List按时间区间部分复制，输入原List、起始时间及结束时间，返回子Lsit
    /// </summary>
    public static class DataListUtils
    {

        public static List<TickFromMssql> ModifyListByTime(List<TickFromMssql> originalList,int startTime,int endTime)
        {
            int startIndex = originalList.FindIndex(s=>s.time>=startTime);
            int endIndex = originalList.FindIndex(s => s.time >= endTime) - 1;
            return originalList.GetRange(startIndex, endIndex - startIndex + 1);
        }

        public static List<TickFromMssql> FillList(List<TickFromMssql> originalList)
        {
            TickFromMssql[] arr = new TickFromMssql[14402];
            foreach (var item in originalList)
            {
                int index = TradeDaysUtils.TimeToIndex(item.time);
                if (index>=0 && index<=14401)
                {
                    arr[index] = item;
                }
            }
            for (int i = 1; i < 14402; i++)
            {
                TickFromMssql thisTick = arr[i];
                if (thisTick==null)
                {
                    arr[i] = arr[i - 1];
                }
            }
            return arr.ToList();
        }


    }

    }
}
