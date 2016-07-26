using BackTestingPlatform.Model;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{
    interface IWsiDAO
    {
        List<WsiData> fetch(string stockCode, DateTime startTime, DateTime endTime);

    }
    class WsiDao : IWsiDAO
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
            for (int i = 0, k = 0; i < len; i++)
            {

                WsiData item = new WsiData();
                item.open = dm[k++];
                item.high = dm[k++];
                item.low = dm[k++];
                item.close = dm[k++];
                items.Add(item);
            }

            return items;
        }
    }
}
