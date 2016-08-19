using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Common;
using BackTestingPlatform.Model.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Stock
{
    class StockDailyRepository : BasicDataRepository<StockDaily>
    {
        protected override List<StockDaily> readFromWind()
        {
            throw new NotImplementedException();
        }


       
        public List<StockDaily> readFromWind(string code,DateTime startDate,DateTime endDate)
        {
            WindAPI w = Platforms.GetWindAPI();
            WindData wd = w.wsd(code, "open,high,low,close,volume,amt", startDate,endDate, "Fill=Previous");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;

            var items = new List<StockDaily>(len * fieldLen);
            if (wd.data is double[])
            {
                double[] dataList = (double[])wd.data;
                DateTime[] timeList = wd.timeList;
                for (int k = 0; k < len; k++)
                {
                    items.Add(new StockDaily
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
