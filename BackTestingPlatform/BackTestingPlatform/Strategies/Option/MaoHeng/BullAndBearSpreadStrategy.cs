using Autofac;
using BackTestingPlatform.AccountOperator.Minute;
using BackTestingPlatform.Charts;
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
using BackTestingPlatform.Model.TALibrary;
using BackTestingPlatform.Strategies.Option.MaoHeng.Model;
using BackTestingPlatform.Transaction;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.SaveResult;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackTestingPlatform.Strategies.Option.MaoHeng
{
    public class BullAndBearSpreadStrategy
    {
        
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startDate, endDate;
        //回测参数设置
        private double initialCapital = 10000;
        private double slipPoint = 0.001;
        private string targetVariety = "510050.SH";
        public BullAndBearSpreadStrategy(int start, int end)
        {
            startDate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
        }
        public void compute()
        {
            

            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startDate), Kit.ToInt_yyyyMMdd(endDate));
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            var optionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
            Caches.put("OptionInfo", optionInfoList);
            
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.initialAssets = initialCapital;
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;

            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();
            List<double> benchmark = new List<double>();
            ///数据准备
            //记录牛市价差两条腿的信息
            BullSpread myLegs = new BullSpread();
            //交易日信息
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startDate, endDate);
            //50ETF的日线数据准备，从回测期开始之前100个交易开始取
            int number = 100;
            List<StockDaily> dailyData = new List<StockDaily>();
            dailyData = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, DateUtils.PreviousTradeDay(startDate,number), endDate);
            //计算50ETF的EMA
            var closePrice = dailyData.Select(x => x.close).ToArray();
            List<double> ema7 = TA_MA.EMA(closePrice, 7).ToList();
            List<double> ema50 = TA_MA.EMA(closePrice, 50).ToList();
            for (int day = 1; day < tradeDays.Count(); day++)
            {
                benchmark.Add(closePrice[day+number]);          
                var today = tradeDays[day];
                myAccount.time = today;
                var dateStructure= OptionUtilities.getDurationStructure(optionInfoList, tradeDays[day]);
                double duration = 0;
                for (int i = 0; i < dateStructure.Count(); i++)
                {
                    if (dateStructure[i]>=20 && dateStructure[i]<=40)
                    {
                        duration = dateStructure[i];
                        break;
                    }
                }
                Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                var etfData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, tradeDays[day]);
                if (ema7[day+number-1]-ema50[day+number-1]>0  && myLegs.strike1==0) // EMA7日线上穿50日线，开牛市价差
                {
                    //取出指定日期
                    double lastETFPrice = dailyData[number + day - 1].close;
                    Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                    dataToday.Add(targetVariety, etfData.Cast<KLine>().ToList());
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), 0);
                    //MinuteSignal openSignal = new MinuteSignal() { code = targetVariety, volume = 10000, time = now, tradingVarieties = "stock", price =averagePrice, minuteIndex = day };
                    //signal.Add(targetVariety, openSignal);
                    //选取指定的看涨期权
                    var list =OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, tradeDays[day], duration),"认购"),lastETFPrice,lastETFPrice+0.5),Kit.ToInt_yyyyMMdd(today));
                    //如果可以构成看涨期权牛市价差，就开仓
                    if (list.Count()>=2)
                    {
                        var option1 = list[0];
                        var option2 = list[list.Count() - 1];
                        var option1Data = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(option1.optionCode, today);
                        var option2Data = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(option2.optionCode, today);
                        if ((option1Data[0].close>0 && option2Data[0].close>0)==true)
                        {
                            dataToday.Add(option1.optionCode, option1Data.Cast<KLine>().ToList());
                            dataToday.Add(option2.optionCode, option2Data.Cast<KLine>().ToList());
                            var vol1 = ImpliedVolatilityUtilities.ComputeImpliedVolatility(option1.strike, duration / 252.0, 0.04, 0, option1.optionType, option1Data[0].close, etfData[0].close);
                            var vol2 = ImpliedVolatilityUtilities.ComputeImpliedVolatility(option2.strike, duration / 252.0, 0.04, 0, option2.optionType, option2Data[0].close, etfData[0].close);
                            MinuteSignal openSignal1 = new MinuteSignal() { code = option1.optionCode, volume = 10000, time = now, tradingVarieties = "option", price = option1Data[0].close, minuteIndex = 0 };
                            MinuteSignal openSignal2 = new MinuteSignal() { code = option2.optionCode, volume = -10000, time = now, tradingVarieties = "option", price = option2Data[0].close, minuteIndex = 0 };
                            signal.Add(option1.optionCode, openSignal1);
                            signal.Add(option2.optionCode, openSignal2);
                            myLegs.code1 = option1.optionCode;
                            myLegs.code2 = option2.optionCode;
                            myLegs.strike1 = option1.strike;
                            myLegs.strike2 = option2.strike;
                            myLegs.endDate = option1.endDate;
                        }
                    }
                    MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now,capitalVerification:false);
                }
                if (positions.Count()>0 && myLegs.strike1 != 0)
                {
                    Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                    dataToday.Add(targetVariety, etfData.Cast<KLine>().ToList());
                    var option1Data = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(myLegs.code1, today);
                    var option2Data = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(myLegs.code2, today);
                    dataToday.Add(myLegs.code1, option1Data.Cast<KLine>().ToList());
                    dataToday.Add(myLegs.code2, option2Data.Cast<KLine>().ToList());
                    int thisIndex = 239;
                    var thisTime = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), thisIndex);
                    var etfPriceNow = etfData[thisIndex].close;
                    var durationNow = DateUtils.GetSpanOfTradeDays(today, myLegs.endDate);
                    if (etfPriceNow > myLegs.strike2 + 0.2 || etfPriceNow < myLegs.strike1-0.2  || durationNow <= 10 )
                    {
                        myLegs = new BullSpread();
                        MinuteCloseAllPositonsWithSlip.closeAllPositions(dataToday, ref positions, ref myAccount, thisTime, slipPoint);
                    }
                    AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, thisTime, dataToday);
                }
                else
                {
                    int thisIndex = 239;
                    var thisTime = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), thisIndex);
                    Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                    dataToday.Add(targetVariety, etfData.Cast<KLine>().ToList());
                    AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, thisTime, dataToday);
                }
                BasicAccount tempAccount = new BasicAccount();
                tempAccount.time = myAccount.time;
                tempAccount.freeCash = myAccount.freeCash;
                tempAccount.margin = myAccount.margin;
                tempAccount.positionValue = myAccount.positionValue;
                tempAccount.totalAssets = myAccount.totalAssets;
                tempAccount.initialAssets = myAccount.initialAssets;
                accountHistory.Add(tempAccount);
            }
            //策略绩效统计及输出
            PerformanceStatisics myStgStats = new PerformanceStatisics();
            myStgStats = PerformanceStatisicsUtils.compute(accountHistory, positions);
            //画图
            Dictionary<string, double[]> line = new Dictionary<string, double[]>();
            double[] netWorth = accountHistory.Select(a => a.totalAssets / initialCapital).ToArray();
            line.Add("NetWorth", netWorth);
            //记录净值数据
            recordToCsv(accountHistory);
            //统计指标在console 上输出
            Console.WriteLine("--------Strategy Performance Statistics--------\n");
            Console.WriteLine(" netProfit:{0,5:F4} \n totalReturn:{1,-5:F4} \n anualReturn:{2,-5:F4} \n anualSharpe :{3,-5:F4} \n winningRate:{4,-5:F4} \n PnLRatio:{5,-5:F4} \n maxDrawDown:{6,-5:F4} \n maxProfitRatio:{7,-5:F4} \n informationRatio:{8,-5:F4} \n alpha:{9,-5:F4} \n beta:{10,-5:F4} \n averageHoldingRate:{11,-5:F4} \n", myStgStats.netProfit, myStgStats.totalReturn, myStgStats.anualReturn, myStgStats.anualSharpe, myStgStats.winningRate, myStgStats.PnLRatio, myStgStats.maxDrawDown, myStgStats.maxProfitRatio, myStgStats.informationRatio, myStgStats.alpha, myStgStats.beta, myStgStats.averageHoldingRate);
            Console.WriteLine("-----------------------------------------------\n");
            //benchmark净值
            List<double> netWorthOfBenchmark = benchmark.Select(x => x / benchmark[0]).ToList();
            line.Add("Base", netWorthOfBenchmark.ToArray());
            string[] datestr = accountHistory.Select(a => a.time.ToString("yyyyMMdd")).ToArray();
             Application.Run(new PLChart(line, datestr));
        }


        private void recordToCsv<T>(IList<T> data)
        {
            var fullPath = ConfigurationManager.AppSettings["CacheData.ResultPath"] + ConfigurationManager.AppSettings["CacheData.StrategyPath"];
            var tag = GetType().FullName;
            var dateStr = Kit.ToInt_yyyyMMdd(DateTime.Now).ToString();
            var type = "account";
            var para = "EMA50_EMA7";
            fullPath = ResultPathUtil.GetLocalPath(fullPath, tag, dateStr, type, para);
            var dt = DataTableUtils.ToDataTable(data);
            CsvFileUtils.WriteToCsvFile(fullPath, dt);
        }
    }
}
