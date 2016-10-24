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
    public class StockDailyRepository : SequentialByYearRepository<StockDaily>
    {
        protected override List<StockDaily> readFromDefaultMssql(string code, DateTime dateStart, DateTime dateEnd, string tag = null, IDictionary<string, object> options = null)
        {
            throw new NotImplementedException();
        }

        protected override List<StockDaily> readFromWind(string code, DateTime dateStart, DateTime dateEnd, string tag = null, IDictionary<string, object> options = null)
        {
            WindAPI w = Platforms.GetWindAPI();
            WindData wd = w.wsd(code, "open,high,low,close,volume,amt,adjfactor,settle,pre_close,pre_settle", dateStart, dateEnd, "Fill=Previous");
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
                        open = dataList[k * fieldLen + 0],
                        high = dataList[k * fieldLen + 1],
                        low = dataList[k * fieldLen + 2],
                        close = dataList[k * fieldLen + 3],
                        volume = dataList[k * fieldLen + 4],
                        amount = dataList[k * fieldLen + 5],
                        adjustFactor= dataList[k * fieldLen + 6],
                        settle=dataList[k*fieldLen+7],
                        preClose=dataList[k*fieldLen+8],
                        preSettle=dataList[k*fieldLen+9]
                    });
                }
            }

            return items;
        }
    }


}
