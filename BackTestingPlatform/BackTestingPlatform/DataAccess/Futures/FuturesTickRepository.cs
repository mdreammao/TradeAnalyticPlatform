using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.Futures
{
    public class FuturesTickRepository : SequentialByDayRepository<FuturesTickFromMssql>
    {
        protected override List<FuturesTickFromMssql> readFromDefaultMssql(string code, DateTime date)
        {
            var connName = "commodityTick";
            var yyyyMM = date.ToString("yyyyMM");
            var yyyyMMdd = date.ToString("yyyyMMdd");
            var codeStr = code.Replace('.', '_');
            var sql = String.Format(@"
            SELECT * FROM [TradeMarket{0}].[dbo].[MarketData_{1}] where tdate={2} order by ttime", yyyyMM, codeStr, yyyyMMdd);
            var connStr = SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
            return dt.AsEnumerable().Select(
                row => new FuturesTickFromMssql
                {
                    code = (string)row["stkcd"],
                    time = Kit.ToDateTime(row["tdate"], row["ttime"]),
                    date = Kit.ToInt(row["tdate"]),
                    ndate=Kit.ToInt(row["ndate"]),
                    moment = Kit.ToInt(row["ttime"]),
                    lastPrice = Kit.ToDouble(row["cp"]),
                    ask = Position.buildAsk(row),
                    bid = Position.buildBid(row),
                    high = Kit.ToDouble(row["hp"]),
                    low = Kit.ToDouble(row["lp"]),
                    preClose = Kit.ToDouble(row["PRECLOSE"]),
                    preSettle = Kit.ToDouble(row["PrevSettle"]),
                    volume = Kit.ToDouble(row["ts"]),
                    amount = Kit.ToDouble(row["tt"])
                }).ToList();
        }

        protected override List<FuturesTickFromMssql> readFromWind(string code, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
