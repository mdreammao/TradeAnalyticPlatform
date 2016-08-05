using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.Option
{
    public class OptionTickDataRepository
    {
        public List<TickFromMssql> fetchRealTimeQuotesFromDatabase(string stockCode, DateTime time, string connName = "corp170")
        {
            var timeStr = time.ToString("yyyyMM");
            var codeStr = stockCode.Substring(0, 8) + '_' + stockCode.Substring(9, 2);
            var sql = String.Format(@"
            SELECT * FROM [WindFullMarket{0}].[dbo].[MarketData_{1}]
            ", timeStr, codeStr);
            if (Convert.ToInt32(timeStr)<201511)
            {
                connName = "corp217";
                sql = String.Format(@"
            SELECT * FROM [TradeMarket{0}].[dbo].[MarketData_{1}]
            ", timeStr, codeStr);
            }
            var connStr=SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
            return dt.AsEnumerable().Select(
                row => new TickFromMssql
                {
                    code = Convert.ToString(row["stkcd"]),
                    date = Convert.ToInt32(row["tdate"]),
                    time = Convert.ToInt32(row["ttime"]),
                    lastPrice = Convert.ToDouble(row["cp"]),
                    ask = _buildPositionAskData(row),
                    bid=_buildPositionBidData(row),
                    highPrice=Convert.ToDouble(row["hp"]),
                    lowPrice=Convert.ToDouble(row["lp"]),
                    preClose=Convert.ToDouble(row["PRECLOSE"]),
                    preSettle = Convert.ToDouble(row["PrevSettle"]),
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
        private DateTime _parseTime(DataRow row)
        {
            throw new NotImplementedException();
        }
    }
}

