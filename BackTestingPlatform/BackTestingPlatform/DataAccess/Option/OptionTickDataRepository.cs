using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.Option
{
    class OptionTickDataRepository
    {
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
                    time = _parseTime(row),
                    cp = Convert.ToDouble(row["cp"]),
                    high = Convert.ToDouble(row["hp"]),
                    low = Convert.ToDouble(row["lp"]),
                    s1 = Convert.ToDouble(row["S1"]),
                    s2 = Convert.ToDouble(row["S2"]),
                    volume = Convert.ToDouble(row["ts"]),
                    amount = Convert.ToDouble(row["tt"])
                }).ToList();

        }

        private DateTime _parseTime(DataRow row)
        {
            throw new NotImplementedException();
        }
    }
}
