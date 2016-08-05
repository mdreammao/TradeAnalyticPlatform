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
        //  private List<TickFromMssql> newDataList;

        public static List<TickFromMssql> CopyFromList(List<TickFromMssql> originalList, int startDate, int endDate)
        {
            int startIndex = originalList.FindIndex(s => s.date == startDate);
            int endIndex = originalList.FindIndex(s => s.date == endDate);

            Console.WriteLine("index1:{0} index2:{1}", startIndex, endIndex);

            if (startIndex == -1 | endIndex == -1)
                return null;
            else
            {
                List<TickFromMssql> newList = originalList.GetRange(startIndex, endIndex - startIndex + 1);
                return newList;
          
            }                
   
        }

    }
}
