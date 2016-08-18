using BackTestingPlatform.Core;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Futures
{
    public class FuturesMinuteKLineRepository : SequentialDataRepository<FuturesMinute>
    {
        protected override List<FuturesMinute> readFromDefaultMssql(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override List<FuturesMinute> readFromWind(string code, DateTime date)
        {
            WindAPI w = Platforms.GetWindAPI();
            DateTime date1 = date.Date, date2 = date.Date.AddDays(1);
            WindData wd = w.wsi(code, "open,high,low,close,volume,amt", date1, date2, "periodstart=09:30:00;periodend=15:00:00;Fill=Previous");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;

            var items = new List<FuturesMinute>(len * fieldLen);
            if (wd.data is double[])
            {
                double[] dataList = (double[])wd.data;
                DateTime[] timeList = wd.timeList;
                for (int k = 0; k < len; k++)
                {
                    items.Add(new FuturesMinute
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
            }

            return items;
        }
    }
}
