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

namespace BackTestingPlatform.Strategies.Stock
{
    public class StockSample01
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startdate, endDate;
        public StockSample01(int start, int end)
        {
            startdate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
        }
        //回测参数设置
        private double initialCapital = 10000000;
        private double slipPoint = 0.005;

        /// <summary>
        /// 50ETF择时策略测试，N-Days Reversion
        /// </summary>
        public void compute()
        {
            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startdate), Kit.ToInt_yyyyMMdd(endDate));

            ///账户初始化
            //初始化positions
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();

            ///数据准备
            //交易日信息
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);
            //50etf分钟数据准备，取全回测期的数据存放于data
            Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
            foreach (var tempDay in tradeDays)
            {
                var ETFData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", tempDay);
                if (!data.ContainsKey("510050.SH"))
                    data.Add("510050.SH", ETFData.Cast<KLine>().ToList());
                else
                    data["510050.SH"].AddRange(ETFData.Cast<KLine>().ToList());
            }

            ///回测循环
            //回测循环--By Day
            foreach (var day in tradeDays)
            {
                //取出当天的数据
                var dataToday = data["510050.SH"].FindAll(s => s.time.Day == day.Day);

                int index = 0;
                //交易开关设置，控制day级的交易开关
                bool tradingOn = true;//总交易开关
                bool openingOn = true;//开仓开关
                bool closingOn = true;//平仓开关

                //是否为回测最后一天
                bool isLastDayOfBackTesting = day.Equals(endDate);

                //回测循环 -- By Minute
                //不允许在同一根1minBar上开平仓
                while (index < 240)
                {
                    int nextIndex = index + 1;
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(day), index);
                    Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();


                    try
                    {
                        //持仓查询，先平后开
                        //若当前有持仓 且 允许平仓
                        //是否是空仓,若position中所有品种volum都为0，则说明是空仓     
                        bool isEmptyPosition = positions.Count != 0 ? positions[positions.Keys.Last()].Values.Sum(x => Math.Abs(x.volume)) == 0 : true;


                        //账户信息更新
                        AccountUpdating.computeAccountUpdating(ref myAccount, ref positions, now, ref data);
                    }

                    catch (Exception)
                    {
                        throw;
                    }

                    index = nextIndex;
                }
                //账户信息记录By Day            
                //用于记录的临时账户
                BasicAccount tempAccount = new BasicAccount();
                tempAccount.time = myAccount.time;
                tempAccount.freeCash = myAccount.freeCash;
                tempAccount.margin = myAccount.margin;
                tempAccount.positionValue = myAccount.positionValue;
                tempAccount.totalAssets = myAccount.totalAssets;
                accountHistory.Add(tempAccount);
            }

            //遍历输出到console
            foreach (var account in accountHistory)
            {
                //这里当账户赋值为NULL时会无限循环
                Console.WriteLine("time:{0},netWorth:{1,8:F3}\n", account.time, account.totalAssets / initialCapital);
            }

            //将accountHistory输出到csv
            var resultPath = ConfigurationManager.AppSettings["CacheData.ResultPath"] + "accountHistory.csv";
            var dt = DataTableUtils.ToDataTable(accountHistory);          // List<MyModel> -> DataTable
            CsvFileUtils.WriteToCsvFile(resultPath, dt);	// DataTable -> CSV File
            Console.ReadKey();
        }
    }
}
