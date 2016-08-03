using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{

    public class KLinesDataRepository
    {

        /// <summary>
        ///  
        /// </summary>
        /// <param name="stockCode">股票代码</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="period">周期(分钟)</param>
        /// <param name="fields">获取字段</param>
        /// <returns></returns>
        public List<KLinesData> fetchFromWind(string stockCode, DateTime startTime, DateTime endTime, int period = 1, string fields = "open,high,low,close,volume,amt")
        {
            var options = String.Format("barSize={0}", period);

            WindAPI wapi = Platforms.GetWindAPI();
            WindData d = wapi.wsi(stockCode, fields, startTime, endTime, options);
            int len = d.timeList.Length;
            int fieldLen = d.fieldList.Length;
            List<KLinesData> items = new List<KLinesData>(len);
            double[] dm = (double[])d.data;
            DateTime[] ttime = d.timeList;

            for (int k = 0; k < len; k += fieldLen)
            {
                items.Add(new KLinesData
                {
                    time = ttime[k],
                    open = dm[k],
                    high = dm[k + 1],
                    low = dm[k + 2],
                    close = dm[k + 3],
                    volume = dm[k + 4],
                    amount = dm[k + 5]
                });
            }

            return items;
        }

        
    }
}
