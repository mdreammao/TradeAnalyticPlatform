using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.DataAccess.Common;
using BackTestingPlatform.Model.Stock;
using WAPIWrapperCSharp;
using BackTestingPlatform.Core;

namespace BackTestingPlatform.DataAccess.Stock
{
    public class StockDailyFactorRepository : SequentialByYearRepository<StockDailyWithFactor>
    {
        protected override List<StockDailyWithFactor> readFromDefaultMssql(string code, DateTime dateStart, DateTime dateEnd, string tag = null, IDictionary<string, object> options = null)
        {
            throw new NotImplementedException();
        }

        protected override List<StockDailyWithFactor> readFromWind(string code, DateTime dateStart, DateTime dateEnd, string tag = null, IDictionary<string, object> options = null)
        {
            WindAPI w = Platforms.GetWindAPI();
            WindData wd = w.wsd(code, "open,high,low,close,volume,amt,adjfactor,settle,pre_close,pre_settle,mkt_cap_CSRC,mkt_cap_float,pe_ttm,pb,ps_ttm,industry2", dateStart, dateEnd, "Fill=Previous;currencyType=;ruleType=3;industryType=1;industryStandard=1");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;

            var items = new List<StockDailyWithFactor>(len * fieldLen);
           // if (wd.data is double[])
            {
                object[] dataList = (object[])wd.data;
                DateTime[] timeList = wd.timeList;

                for (int k = 0; k < len; k++)
                {
                    items.Add(new StockDailyWithFactor
                    {
                        time = timeList[k],
                        open = (double)dataList[k * fieldLen + 0],
                        high = (double)dataList[k * fieldLen + 1],
                        low = (double)dataList[k * fieldLen + 2],
                        close = (double)dataList[k * fieldLen + 3],
                        volume = (double)dataList[k * fieldLen + 4],
                        amount = (double)dataList[k * fieldLen + 5],
                        adjustFactor = (double)dataList[k * fieldLen + 6],
                        settle = Convert.IsDBNull(dataList[k * fieldLen + 7])?0:(double)dataList[k * fieldLen + 7],
                        preClose = (double)dataList[k * fieldLen + 8],
                        preSettle = Convert.IsDBNull(dataList[k * fieldLen + 9])?0:(double)dataList[k * fieldLen + 9],
                        marketValue = (double)dataList[k * fieldLen + 10],
                        floatMarketValue = (double)dataList[k * fieldLen + 11],
                        PE = (double)dataList[k * fieldLen + 12],
                        PB = (double)dataList[k * fieldLen + 13],
                        PS = (double)dataList[k * fieldLen + 14],
                        industry = (string)dataList[k * fieldLen + 15]
                    });
                }
            }

            return items;
        }
    }
}
