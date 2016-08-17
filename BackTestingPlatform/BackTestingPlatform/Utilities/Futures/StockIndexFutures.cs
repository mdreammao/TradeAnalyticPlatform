using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.Futures
{
    public static class StockIndexFutures
    {
        public static string GetCodeByDate(string code,DateTime date)
        {
            DateTime lastTradingDate =DateUtils.NextOrCurrentTradeDay(GetLastTradingDayOfThisMonth(date));
            if (date<=lastTradingDate)
            {
                code+=date.ToString("yyMM") + ".CFE";
            }
            else
            {
                code += date.AddMonths(1).ToString("yyMM") + ".CFE";
            }
            return code;
        }

        public static DateTime GetLastTradingDayOfThisMonth(DateTime date)
        {
            return date;
        }

        

    }
}
