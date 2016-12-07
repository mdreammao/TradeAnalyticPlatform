using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.DataApplication
{
    
    public static class KLineDataUtils
    {
        /// <summary>
        /// //查缺补漏函数：
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="orignalList"></param>
        /// <returns></returns>
        public static List<T> leakFilling<T>(List<T> orignalList) where T : KLine, new()
        {
            for (int i = 0; i < orignalList.Count(); i++)
            {
                var KLine0 = orignalList[i];
                if (double.IsNaN(KLine0.close)==true)
                {
                    orignalList[i] = orignalList[i - 1];
                }
            }
            return orignalList;
        }
    }
}
