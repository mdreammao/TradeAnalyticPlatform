using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{

    public class StockTickDataRepository
    {
        /// <summary>
        ///  
        /// </summary>
        /// <param name="stockCode">股票代码，例如510050.SH</param>
        /// <param name="startTime">起始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="period">周期(分钟)</param>
        /// <param name="fields">获取字段</param>
        /// <returns></returns>
        public List<RealTimeQuotes> fetchRealTimeQuotesFromWind(string stockCode)
        {

            WindAPI wapi = Platforms.GetWindAPI();

            WindData d = wapi.wsq(stockCode, "rt_last_vol,rt_latest,rt_vol,rt_amt,rt_chg,rt_pct_chg,rt_high_limit,rt_low_limit,rt_swing,rt_vol_ratio,rt_turn,rt_mkt_cap,rt_oi_chg,rt_pre_close_dp,rt_ask1,rt_ask2,rt_bid1,rt_bid2,rt_bsize1,rt_bsize2,rt_delta,rt_imp_volatility", "");

            //todo 对d进行处理
            return null;
        }


        public List<RealTimeQuotes> fetchRealTimeQuotesFromDatabase(string stockCode, DateTime time, string connName = "corp170")
        {
            var timeStr = time.ToString("yyyyMM");
            var codeStr = stockCode.Substring(0, 6) + '_' + stockCode.Substring(7, 2);
            var sql = String.Format(@"
            SELECT * FROM [WindFullMarket{0}].[dbo].[MarketData_{1}]
            ", timeStr, codeStr);
            var connStr=SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
            return dt.AsEnumerable().Select(
                row => new RealTimeQuotes
                {
                    time = Kit.ToDateTime((string)row["tdate"],(string)row["ttime"]),
                    cp = Convert.ToDouble(row["cp"]),
                    high = Convert.ToDouble(row["hp"]),
                    low = Convert.ToDouble(row["lp"]),
                    s1 = Convert.ToDouble(row["S1"]),
                    s2 = Convert.ToDouble(row["S2"]),
                    volume = Convert.ToDouble(row["ts"]),
                    amount = Convert.ToDouble(row["tt"])
                }).ToList();
        }

        public List<TickFromMssql> fetchDataFromMssql(string stockCode, DateTime time, string connName = "corp170")
        {
            var timeStr = time.ToString("yyyyMM");
            var codeStr = stockCode.Substring(0, 6) + '_' + stockCode.Substring(7, 2);
            var sql = String.Format(@"
            SELECT * FROM [WindFullMarket{0}].[dbo].[MarketData_{1}]
            ", timeStr, codeStr);
            var connStr = SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
           
            return dt.AsEnumerable().Select(
                row => new TickFromMssql
                {                  
                    code = Convert.ToString(row["stkcd"]),
                    date = Convert.ToInt32(row["tdate"]),
                    time = Convert.ToInt32(row["ttime"]),
                    lastPrice = Convert.ToDouble(row["cp"]),
              //    ask = _buildPositionAskData(row),
              //    bid = _buildPositionBidData(row),
                    highPrice = Convert.ToDouble(row["hp"]),
                    lowPrice = Convert.ToDouble(row["lp"]),
              //    preClose = Convert.ToDouble(row["PRECLOSE"]),
              //    preSettle = Convert.ToDouble(row["PrevSettle"]),
                    volume = Convert.ToDouble(row["ts"]),
                    turnoverVolume = Convert.ToDouble(row["tt"])
                }).ToList();
        }
        private PositionData[] _buildPositionAskData(DataRow row)
        {
            PositionData[] res = new PositionData[5];
            res[0] = new PositionData(Convert.ToDouble(row["S1"]), Convert.ToDouble(row["SV1"]));
            res[1] = new PositionData(Convert.ToDouble(row["S2"]), Convert.ToDouble(row["SV2"]));
            res[2] = new PositionData(Convert.ToDouble(row["S3"]), Convert.ToDouble(row["SV3"]));
            res[3] = new PositionData(Convert.ToDouble(row["S4"]), Convert.ToDouble(row["SV4"]));
            res[4] = new PositionData(Convert.ToDouble(row["S5"]), Convert.ToDouble(row["SV5"]));
            return res;
        }

        private PositionData[] _buildPositionBidData(DataRow row)
        {
            PositionData[] res = new PositionData[5];
            res[0] = new PositionData(Convert.ToDouble(row["B1"]), Convert.ToDouble(row["BV1"]));
            res[1] = new PositionData(Convert.ToDouble(row["B2"]), Convert.ToDouble(row["BV2"]));
            res[2] = new PositionData(Convert.ToDouble(row["B3"]), Convert.ToDouble(row["BV3"]));
            res[3] = new PositionData(Convert.ToDouble(row["B4"]), Convert.ToDouble(row["BV4"]));
            res[4] = new PositionData(Convert.ToDouble(row["B5"]), Convert.ToDouble(row["BV5"]));
            return res;
        }

    }


}
