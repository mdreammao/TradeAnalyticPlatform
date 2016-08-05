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
            var connStr=SqlUtils.GetConnectionString(connName);
            DataTable dt = SqlUtils.GetTable(connStr, sql);
            return dt.AsEnumerable().Select(
                row => new TickFromMssql
                {
                   // date = (Convert.IsDBNull(row["tdate"]) | Convert.ToInt32(row["tdate"])<0) ? 0 :  Convert.ToInt32(row["tdate"]),
                    date = Convert.ToInt32(row["tdate"]),
                    time = Convert.ToInt32(row["ttime"]),
                    lastPrice = Convert.ToDouble(row["cp"]),
                    ask = _buildPositionAskData(row),
                    bid=_buildPositionBidData(row),
                    highPrice=Convert.ToDouble(row["hp"]),
                    lowPrice=Convert.ToDouble(row["lp"]),
                    highLimit=Convert.ToDouble(row["HighLimit"]),
                    lowLimit=Convert.ToDouble(row["LowLight"]),
                    preClose=Convert.ToDouble(row["PRECLOSE"]),
                    volume = Convert.ToDouble(row["ts"]),
                    turnoverVolume = Convert.ToDouble(row["tt"])
                }).ToList();

        }

        private PositionData[] _buildPositionAskData(DataRow row)
        {
            PositionData[] res = new PositionData[10];
            res[0] = new PositionData(Convert.ToDouble(row["S1"]), Convert.ToDouble(row["SV1"]));
            res[1] = new PositionData(Convert.ToDouble(row["S2"]), Convert.ToDouble(row["SV2"]));
            res[2] = new PositionData(Convert.ToDouble(row["S3"]), Convert.ToDouble(row["SV3"]));
            res[3] = new PositionData(Convert.ToDouble(row["S4"]), Convert.ToDouble(row["SV4"]));
            res[4] = new PositionData(Convert.ToDouble(row["S5"]), Convert.ToDouble(row["SV5"]));
            res[5] = new PositionData(Convert.ToDouble(row["S6"]), Convert.ToDouble(row["SV6"]));
            res[6] = new PositionData(Convert.ToDouble(row["S7"]), Convert.ToDouble(row["SV7"]));
            res[7] = new PositionData(Convert.ToDouble(row["S8"]), Convert.ToDouble(row["SV8"]));
            res[8] = new PositionData(Convert.ToDouble(row["S9"]), Convert.ToDouble(row["SV9"]));
            res[9] = new PositionData(Convert.ToDouble(row["S10"]), Convert.ToDouble(row["SV10"]));
            return res;
        }

        private PositionData[] _buildPositionBidData(DataRow row)
        {
            PositionData[] res = new PositionData[10];
            res[0] = new PositionData(Convert.ToDouble(row["B1"]), Convert.ToDouble(row["BV1"]));
            res[1] = new PositionData(Convert.ToDouble(row["B2"]), Convert.ToDouble(row["BV2"]));
            res[2] = new PositionData(Convert.ToDouble(row["B3"]), Convert.ToDouble(row["BV3"]));
            res[3] = new PositionData(Convert.ToDouble(row["B4"]), Convert.ToDouble(row["BV4"]));
            res[4] = new PositionData(Convert.ToDouble(row["B5"]), Convert.ToDouble(row["BV5"]));
            res[5] = new PositionData(Convert.ToDouble(row["B6"]), Convert.ToDouble(row["BV6"]));
            res[6] = new PositionData(Convert.ToDouble(row["B7"]), Convert.ToDouble(row["BV7"]));
            res[7] = new PositionData(Convert.ToDouble(row["B8"]), Convert.ToDouble(row["BV8"]));
            res[8] = new PositionData(Convert.ToDouble(row["B9"]), Convert.ToDouble(row["BV9"]));
            res[9] = new PositionData(Convert.ToDouble(row["B10"]), Convert.ToDouble(row["BV10"]));
            return res;
        }
        private DateTime _parseTime(DataRow row)
        {
            throw new NotImplementedException();
        }
    }
}
