using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Utilities.TALibrary; 

namespace BackTestingPlatform.Monitor.MDCE
{
    public class MDCE
    {
        private DateTime today;
        public MDCE(int todayInt=0)
        {
            if (todayInt == 0)
            {
                today = DateTime.Now;
            }
            else
            {
                today = Kit.ToDate(todayInt);
            }
        }

        public void compute()
        {
            var list = getHistoricalDailyData("M1705.DCE");
            var mdata = (from x in list where x.close > 0 select x).ToList();
            var timelist = mdata.Select(x => x.time).ToArray();
            var closelist = mdata.Select(x => x.close).ToArray();
            var tenth = Volatility.HVYearly(closelist, 30);
        }

        private List<FuturesDaily> getHistoricalDailyData(string code)
        {
            return Platforms.container.Resolve<FuturesDailyRepository>().fetchFromLocalCsvOrWindAndSave(code, today.AddDays(-360), today.AddDays(-1));
        }
    }
}
