using BackTestingPlatform.Core;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Option
{
    public class OptionMinuteDataRepository
    {
        public List<OptionMinuteData> fetchMinuteDataFromWind(string stockCode, DateTime time)
        {
            WindAPI w = Platforms.GetWindAPI();
            var timeStr = time.ToString("yyyyMM");
            DateTime start = Convert.ToDateTime(time.ToString("yyyy-MM-dd 00:00:00"));
            DateTime end = Convert.ToDateTime(time.ToString("yyyy-MM-dd 23:59:59"));
            WindData wd = w.wsi(stockCode.ToUpper(), "open,high,low,close,volume,amt", "2016-08-05 09:00:00", "2016-08-06 21:16:19", "periodstart=09:30:00;periodend=15:00:00;Fill=Previous");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<OptionMinuteData> items = new List<OptionMinuteData>(len * fieldLen);
            double[] dataList = (double[])wd.data;
            DateTime[] timeList = wd.timeList;
            for (int k = 0; k < len; k++)
            {
                items.Add(new OptionMinuteData
                {
                    time = timeList[k],
                    open = (double)dataList[k * fieldLen + 0],
                    high = (double)dataList[k * fieldLen + 1],
                    low = (double)dataList[k * fieldLen + 2],
                    close = (double)dataList[k * fieldLen + 3],
                    volume = (double)dataList[k * fieldLen + 4],
                    amount = (double)dataList[k * fieldLen + 5]
                });
            }
            return items;
        }
    }
}
