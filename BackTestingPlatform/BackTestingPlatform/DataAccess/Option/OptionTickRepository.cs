using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.Option
{
    public class OptionTickRepository : SequentialByDayRepository<OptionTickFromMssql>
    {
        protected override List<OptionTickFromMssql> readFromDefaultMssql(string code, DateTime date)
        {
            var connName = "corp170";
            var yyyyMM = date.ToString("yyyyMM");
            var yyyyMMdd = date.ToString("yyyyMMdd");
            var codeStr = code.Replace('.', '_');
            var sql = String.Format(@"
            SELECT * FROM [WindFullMarket{0}].[dbo].[MarketData_{1}] where tdate={2} ORDER BY ttime
            ", yyyyMM, codeStr, yyyyMMdd);
            if (Convert.ToInt32(yyyyMM) < 201511)
            {
                connName = "corp217";
                sql = String.Format(@"
            SELECT * FROM [TradeMarket{0}].[dbo].[MarketData_{1}] where tdate={2} ORDER BY ttime
            ", yyyyMM, codeStr, yyyyMMdd);
            }

            var connStr = SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
            return dt.AsEnumerable().Select(
                row => new OptionTickFromMssql
                {
                    code = (string)row["stkcd"],
                    time = Kit.ToDateTime(row["tdate"], row["ttime"]),
                    date = Kit.ToInt(row["tdate"]),
                    moment = Kit.ToInt(row["ttime"]),
                    lastPrice = Kit.ToDouble(row["cp"]),
                    ask = Position.buildAsk5(row),
                    bid = Position.buildBid5(row),
                    high = Kit.ToDouble(row["hp"]),
                    low = Kit.ToDouble(row["lp"]),
                    preClose = Kit.ToDouble(row["PRECLOSE"]),
                    preSettle = Kit.ToDouble(row["PrevSettle"]),
                    volume = Kit.ToDouble(row["ts"]),
                    amount = Kit.ToDouble(row["tt"])
                }).ToList();
        }

        protected override List<OptionTickFromMssql> readFromWind(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<OptionTickFromMssql> fetchFromLocalCsvOrMssqlAndResampleAndSave(string code, DateTime date, TimeLine timeline)
        {
            var data = fetchFromLocalCsv(code, date, "OptionTickFromMssql.Resampled");
            bool csvFound = (data != null);
            data = fetchFromMssql(code, date, "OptionTickFromMssql.Resampled");
            var data2 = SequentialUtils.ResampleAndAlign(data, timeline, date);
            if (!csvFound && data != null)
                saveToLocalCsv(data2, code, date, "OptionTickFromMssql.Resampled");
            return data2;
        }
    }


}
