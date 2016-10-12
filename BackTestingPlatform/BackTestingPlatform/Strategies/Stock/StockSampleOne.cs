using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Stock
{
    class StockSampleOne
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startdate, endDate;
        private int MA, MAshort, MAlong;

        /// <summary>
        /// 海龟算法
        /// </summary>
        /// <param name="start">回测开始时间</param>
        /// <param name="end">回测结束时间</param>
        /// <param name="MA">开仓指标观察期</param>
        /// <param name="MAshort">短均线</param>
        /// <param name="MAlong">长均线</param>
        public StockSampleOne(int start,int end,int MA,int MAshort,int MAlong)
        {
            startdate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
            this.MA = MA;
            this.MAshort = MAshort;
            this.MAlong = MAlong;
        }

        /// <summary>
        /// 50ETF的海龟测试
        /// </summary>
        public void Compute()
        {
            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startdate), Kit.ToInt_yyyyMMdd(endDate));
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);

            var ETFDaily = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH",startdate, endDate);

        }

    }
}
