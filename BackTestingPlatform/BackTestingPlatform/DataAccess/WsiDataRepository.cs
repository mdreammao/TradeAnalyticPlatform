using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{
    public interface WsiDataRepository
    {
        List<WsiData> fetch(string stockCode, DateTime startTime, DateTime endTime);

    }
    /// <summary>
    /// 从万德API获取数据的实现
    /// </summary>
    public class WsiDataRepositoryFromWind : WsiDataRepository
    {
        public List<WsiData> fetch(string stockCode, DateTime startTime, DateTime endTime)
        {

            var fields = "open, high, low, close";
            var options = "";
            //WindAPI api = MyPlatform.currentContext().getWindAPI();
            WindAPI api = new WindAPI();
            api.start();

            WindData d = api.wsi(stockCode, fields, startTime, endTime, options);
            int len = d.timeList.Length;

            //build target data structrue
            List<WsiData> items = new List<WsiData>(len);
            double[] dm = (double[])d.data;
            for (int i = 0, k = 0; i < len; i++, k += 4)
            {
                items.Add(new WsiData
                {
                    open = dm[k],
                    high = dm[k + 1],
                    low = dm[k + 2],
                    close = dm[k + 3]
                });
            }

            return items;
        }
    }
}
