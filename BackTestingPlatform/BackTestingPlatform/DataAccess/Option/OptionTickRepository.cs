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
                    amount = Kit.ToDouble(row["tt"]),
                    ask1 = Kit.ToDouble(row["S1"]),
                    ask2 = Kit.ToDouble(row["S2"]),
                    ask3 = Kit.ToDouble(row["S3"]),
                    ask4 = Kit.ToDouble(row["S4"]),
                    ask5 = Kit.ToDouble(row["S5"]),
                    askv1 = Kit.ToDouble(row["SV1"]),
                    askv2 = Kit.ToDouble(row["SV2"]),
                    askv3 = Kit.ToDouble(row["SV3"]),
                    askv4 = Kit.ToDouble(row["SV4"]),
                    askv5 = Kit.ToDouble(row["SV5"]),
                    bid1 = Kit.ToDouble(row["B1"]),
                    bid2 = Kit.ToDouble(row["B2"]),
                    bid3 = Kit.ToDouble(row["B3"]),
                    bid4 = Kit.ToDouble(row["B4"]),
                    bid5 = Kit.ToDouble(row["B5"]),
                    bidv1 = Kit.ToDouble(row["BV1"]),
                    bidv2 = Kit.ToDouble(row["BV2"]),
                    bidv3 = Kit.ToDouble(row["BV3"]),
                    bidv4 = Kit.ToDouble(row["BV4"]),
                    bidv5 = Kit.ToDouble(row["BV5"])
                }).ToList();
        }

        

        protected override List<OptionTickFromMssql> readFromWind(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<OptionTickFromMssql> fetchFromLocalCsvOrMssqlAndResampleAndSave(string code, DateTime date, TimeLine timeline)
        {
            var data = fetchFromLocalCsv(code, date, "OptionTickFromMssql.Resampled");
            //从csv中取出数据后，需要整理bid及ask数据
            if (data!=null)
            {
                foreach (var item in data)
                {
                    if (item.ask==null)
                    {
                        item.ask =new Position[5];
                        item.ask[0] = new Position(item.ask1, item.askv1);
                        item.ask[1] = new Position(item.ask2, item.askv2);
                        item.ask[2] = new Position(item.ask3, item.askv3);
                        item.ask[3] = new Position(item.ask4, item.askv4);
                        item.ask[4] = new Position(item.ask5, item.askv5);

                    }
                    if (item.bid == null)
                    {
                        item.bid = new Position[5];
                        item.bid[0] = new Position(item.bid1, item.bidv1);
                        item.bid[1] = new Position(item.bid2, item.bidv2);
                        item.bid[2] = new Position(item.bid3, item.bidv3);
                        item.bid[3] = new Position(item.bid4, item.bidv4);
                        item.bid[4] = new Position(item.bid5, item.bidv5);
                    }
                }
            }
            bool csvFound = (data != null);
            data = fetchFromMssql(code, date, "OptionTickFromMssql.Resampled");
            var data2 = SequentialUtils.ResampleAndAlign(data, timeline, date);
            if (!csvFound && data != null)
                saveToLocalCsv(data2, code, date, "OptionTickFromMssql.Resampled");
            return data2;
        }
    }


}
