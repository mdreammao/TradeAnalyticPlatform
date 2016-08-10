using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
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
        public List<TickFromMssql> fetchDataFromMssql(string stockCode, DateTime time, string connName = "corp170")
        {
            var timeStr = time.ToString("yyyyMM");
            var codeStr = stockCode.Substring(0, 8) + '_' + stockCode.Substring(9, 2);
            var sql = String.Format(@"
            SELECT * FROM [WindFullMarket{0}].[dbo].[MarketData_{1}]
            ", timeStr, codeStr);
            if (Convert.ToInt32(timeStr) < 201511)
            {
                connName = "corp217";
                sql = String.Format(@"
            SELECT * FROM [TradeMarket{0}].[dbo].[MarketData_{1}]
            ", timeStr, codeStr);
            }
            var connStr = SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
            return dt.AsEnumerable().Select(
                row => new TickFromMssql
                {
                    code = (string)row["stkcd"],
                    date = Kit.ToInt(row["tdate"]),
                    time = Kit.ToInt(row["ttime"]),
                    lastPrice = Kit.ToDouble(row["cp"]),
                    ask = _buildPositionAskData(row),
                    bid = _buildPositionBidData(row),
                    highPrice = Kit.ToDouble(row["hp"]),
                    lowPrice = Kit.ToDouble(row["lp"]),
                    preClose = Kit.ToDouble(row["PRECLOSE"]),
                    preSettle = Kit.ToDouble(row["PrevSettle"]),
                    volume = Kit.ToDouble(row["ts"]),
                    turnoverVolume = Kit.ToDouble(row["tt"])
                }).ToList();

        }

        PositionData[] _buildPositionAskData(DataRow row)
        {
            PositionData[] res = new PositionData[5];
            res[0] = new PositionData(Kit.ToDouble(row["S1"]), Kit.ToDouble(row["SV1"]));
            res[1] = new PositionData(Kit.ToDouble(row["S2"]), Kit.ToDouble(row["SV2"]));
            res[2] = new PositionData(Kit.ToDouble(row["S3"]), Kit.ToDouble(row["SV3"]));
            res[3] = new PositionData(Kit.ToDouble(row["S4"]), Kit.ToDouble(row["SV4"]));
            res[4] = new PositionData(Kit.ToDouble(row["S5"]), Kit.ToDouble(row["SV5"]));
            return res;
        }

        PositionData[] _buildPositionBidData(DataRow row)
        {
            return new PositionData[5]{
                new PositionData(Kit.ToDouble(row["B1"]), Kit.ToDouble(row["BV1"])),
                new PositionData(Kit.ToDouble(row["B2"]), Kit.ToDouble(row["BV2"])),
                new PositionData(Kit.ToDouble(row["B3"]), Kit.ToDouble(row["BV3"])),
                new PositionData(Kit.ToDouble(row["B4"]), Kit.ToDouble(row["BV4"])),
                new PositionData(Kit.ToDouble(row["B5"]), Kit.ToDouble(row["BV5"]))
            };
        }

    }
}

