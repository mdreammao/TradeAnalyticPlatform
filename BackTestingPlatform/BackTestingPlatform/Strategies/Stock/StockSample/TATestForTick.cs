using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Transaction;
using BackTestingPlatform.Transaction.TransactionWithSlip;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Strategies.Stock.StockSample;
using BackTestingPlatform.Strategies.Stock.StockSample01;
using BackTestingPlatform.Utilities.Common;
using TicTacTec.TA.Library;
using System.Diagnostics;
using BackTestingPlatform.Model.TALibrary;
using BackTestingPlatform.Model.Futures;

namespace BackTestingPlatform.Strategies.Stock.StockSample
{
    public class TATestForTick
    {
        //策略参数
        public const int periods = 5;

        static Logger log = LogManager.GetCurrentClassLogger();
        public DateTime startDate, endDate;
        public TATestForTick(int start, int end)
        {
            startDate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
        }
        string targetVariety = "510050.SH";

        public void compute()
        {

            ///数据准备
            //交易日信息
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startDate, endDate);
            //50etf分钟数据准备，取全回测期的数据存放于data
            Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
            foreach (var tempDay in tradeDays)
            {
                var ETFData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, tempDay);
                if (!data.ContainsKey(targetVariety))
                    data.Add(targetVariety, ETFData.Cast<KLine>().ToList());
                else
                    data[targetVariety].AddRange(ETFData.Cast<KLine>().ToList());
            }

            var tick = Platforms.container.Resolve<StockTickRepository>().fetchFromMssql(targetVariety, Kit.ToDate(20160905));
            List<StockTickFromMssql> tick2 = SequentialUtils.ResampleAndAlign(tick, Constants.timeline500ms, Kit.ToDate(20160905));


            ///指标计算
            var closePrice = tick2.Select(x => x.bid[1].price).ToArray();

            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start(); //  开始监视代码运行时间
            var MAValue = TA_MA.SMA(closePrice, periods);
            stopwatch2.Stop(); //  停止监视
            TimeSpan timespan2 = stopwatch2.Elapsed; //  获取当前实例测量得出的总时间
            Console.WriteLine("Calc MA2 Running Time: {0}", timespan2.TotalSeconds);
            Console.ReadKey();

        }


    }
}
