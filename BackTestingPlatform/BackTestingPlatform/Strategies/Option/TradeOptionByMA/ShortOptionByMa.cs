using Autofac;
using BackTestingPlatform.Charts;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.SaveResult.Common;
using BackTestingPlatform.Utilities.SaveResult.Option;
using BackTestingPlatform.Utilities.TALibrary;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Strategies.Option.TradeOptionByMA.Model;
using BackTestingPlatform.Transaction.Minute.maoheng;
using BackTestingPlatform.AccountOperator.Minute.maoheng;

namespace BackTestingPlatform.Strategies.Option.TradeOptionByMA
{
    public class ShortOptionByMA
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startDate, endDate;
        List<DateTime> tradeDays = new List<DateTime>();
        List<OptionInfo> optionInfoList = new List<OptionInfo>();

        //回测参数设置
        private double initialCapital = 10000;
        private double slipPoint = 0.001;
        private string targetVariety = "510050.SH";
        private double motion = 0.12;
        private List<NetValue> netValue = new List<NetValue>();
        //构造函数
        public ShortOptionByMA(int start, int end)
        {
            startDate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
            //交易日信息
            tradeDays = DateUtils.GetTradeDays(startDate, endDate);
            compute();
        }
        public void compute()
        {
            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startDate), Kit.ToInt_yyyyMMdd(endDate));
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            optionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
            Caches.put("OptionInfo", optionInfoList);
            //初始化头寸信息
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.initialAssets = initialCapital;
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //初始化持仓信息
            StranglePair holdPair = new StranglePair();
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();
            List<double> benchmark = new List<double>();
            //可以变动的策略参数
            int length = 40;
            double range = 0.04;

            //50ETF的日线数据准备，从回测期开始之前100个交易开始取
            int number = 100;
            List<StockDaily> dailyData = new List<StockDaily>();
            dailyData = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, DateUtils.PreviousTradeDay(startDate, number), endDate);
            var closePrice = dailyData.Select(x => x.close).ToArray();
            var ETFMA = TA_MA.SMA(closePrice, length);
            //获取中国波指的数据
            List<StockDaily>  iVix = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("000188.SH", startDate, endDate);
            //按交易日回测
            for (int day = 0; day < tradeDays.Count(); day++)
            {
                benchmark.Add(closePrice[day + number]);
                double MAyesterday = ETFMA[day + number - 1];
                double lastETFPrice = dailyData[number + day - 1].close;
                var today = tradeDays[day];
                //获取当日上市的期权合约列表
                var optionInfoList = OptionUtilities.getUnmodifiedOptionInfoList(this.optionInfoList, today);
                //初始化信号的数据结构
                Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                //获取今日日内50ETF数据
                var etfData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, tradeDays[day]);
                //初始化行情信息，将50ETF的价格放入dataToday
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                dataToday.Add(targetVariety, etfData.Cast<KLine>().ToList());
                //记录今日账户信息
                myAccount.time = today;

                //获取今日期权的到期日期
                var dateStructure = OptionUtilities.getDurationStructure(optionInfoList, tradeDays[day]);
                //选定到日期在40个交易日至60个交易日的合约
                double duration = 0;
                //for (int i = 0; i < dateStructure.Count(); i++)
                //{
                //    if (dateStructure[i] >= 40 && dateStructure[i] <= 80)
                //    {
                //        duration = dateStructure[i];
                //        break;
                //    }
                //}
                duration = dateStructure[1];
                //如果没有持仓就开仓
                if (holdPair.endDate==new DateTime())
                {
                    if (duration==0)
                    {
                        continue;
                    }
                    //按照开盘1分钟的价格来开平仓。
                    int index = 0;
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), index);
                    double etfPriceNow = etfData[index].open;
                    if (etfPriceNow > MAyesterday * (1 + range))//看涨，卖出虚值的put
                    {
                        var list = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽"), etfPriceNow-0.1, etfPriceNow - 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => -x.strike).ToList();
                        if (list.Count==0)
                        {
                            continue;
                        }
                        OptionInfo put = list[0];
                        if (put.strike != 0 && (put.modifiedDate > today.AddDays(10) || put.modifiedDate < today)) //开仓
                        {
                            tradeAssistant(ref dataToday, ref signal, put.optionCode, -put.contractMultiplier, today, now, index);
                            holdPair = new StranglePair() { callCode = "", putCode = put.optionCode, callPosition = 0, putPosition = -put.contractMultiplier, endDate =put.endDate, etfPrice = etfPriceNow, callStrike = 0, putStrike = put.strike};
                            MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
                        }
                    }
                    else if (etfPriceNow < MAyesterday * (1 - range))//看跌，卖出虚值的call
                    {
                        var list = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购"), etfPriceNow + 0.1, etfPriceNow + 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => x.strike).ToList();
                        if (list.Count == 0)
                        {
                            continue;
                        }
                        OptionInfo call = list[0];
                        if (call.strike != 0 && (call.modifiedDate > today.AddDays(10) || call.modifiedDate < today)) //开仓
                        {
                            tradeAssistant(ref dataToday, ref signal, call.optionCode, -call.contractMultiplier, today, now, index);
                            holdPair = new StranglePair() { callCode = call.optionCode, putCode = "", callPosition = -call.contractMultiplier, putPosition = 0, endDate = call.endDate, etfPrice = etfPriceNow, callStrike = call.strike, putStrike = 0 };
                            MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
                        }
                    }
                    else//不涨不跌，卖出宽跨式期权
                    {
                        var putList = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽"), etfPriceNow - 0.1, etfPriceNow - 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => -x.strike).ToList();
                        
                        var callList = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购"), etfPriceNow + 0.1, etfPriceNow + 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => x.strike).ToList();
                        if (putList.Count == 0 || callList.Count==0)
                        {
                            continue;
                        }
                        OptionInfo call = callList[0];
                        OptionInfo put = putList[0];
                        if (put.strike != 0 && (put.modifiedDate > today.AddDays(10) || put.modifiedDate < today) &&call.strike != 0 && (call.modifiedDate > today.AddDays(10) || call.modifiedDate < today)) //开仓
                        {
                            tradeAssistant(ref dataToday, ref signal, put.optionCode, -put.contractMultiplier, today, now, index);
                            tradeAssistant(ref dataToday, ref signal, call.optionCode, -call.contractMultiplier, today, now, index);
                            holdPair = new StranglePair() { callCode = call.optionCode, putCode = put.optionCode, callPosition = -call.contractMultiplier, putPosition = -put.contractMultiplier, endDate = put.endDate, etfPrice = etfPriceNow, callStrike = call.strike, putStrike = put.strike };
                            MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
                        }
                    }
                }
                else //如果有持仓就判断需不需要移仓
                {
                    double durationNow = DateUtils.GetSpanOfTradeDays(today, holdPair.endDate);
                    int index = 234;
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), index);
                    if (holdPair.callPosition != 0)
                    {
                        tradeAssistant(ref dataToday, ref signal, holdPair.callCode, -holdPair.callPosition, today, now, index);
                    }
                    if (holdPair.putPosition != 0)
                    {
                        tradeAssistant(ref dataToday, ref signal, holdPair.putCode, -holdPair.putPosition, today, now, index);
                    }
                    if (durationNow<=10) //强制平仓
                    {
                        //按照收盘前5分钟的价格来开平仓。
                        MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
                        holdPair = new StranglePair();
                    }
                }
                
                if (etfData.Count > 0)
                {
                    //更新当日属性信息
                   AccountUpdatingWithMinuteBar.computeAccount(ref myAccount, positions, etfData.Last().time, etfData.Count() - 1, dataToday);
                    //记录历史仓位信息
                    accountHistory.Add(new BasicAccount(myAccount.time, myAccount.totalAssets, myAccount.freeCash, myAccount.positionValue, myAccount.margin, myAccount.initialAssets));
                    benchmark.Add(etfData.Last().close);
                    if (netValue.Count() == 0)
                    {
                        netValue.Add(new NetValue { time = today, netvalueReturn = 0, benchmarkReturn = 0, netvalue = myAccount.totalAssets, benchmark = etfData.Last().close });
                    }
                    else
                    {
                        var netValueLast = netValue.Last();
                        netValue.Add(new NetValue { time = today, netvalueReturn = myAccount.totalAssets / netValueLast.netvalue - 1, benchmarkReturn = etfData.Last().close / netValueLast.benchmark - 1, netvalue = myAccount.totalAssets, benchmark = etfData.Last().close });
                    }

                }
            }
            //策略绩效统计及输出
            PerformanceStatisics myStgStats = new PerformanceStatisics();
            myStgStats = PerformanceStatisicsUtils.compute(accountHistory, positions);
            //画图
            Dictionary<string, double[]> line = new Dictionary<string, double[]>();
            double[] netWorth = accountHistory.Select(a => a.totalAssets / initialCapital).ToArray();
            line.Add("NetWorth", netWorth);
            //记录净值数据
            RecordUtil.recordToCsv(accountHistory, GetType().FullName, "account", parameters: "ShortOptionByMA", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录持仓变化
            var positionStatus = OptionRecordUtil.Transfer(positions);
            RecordUtil.recordToCsv(positionStatus, GetType().FullName, "positions", parameters: "ShortOptionByMA", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录统计指标
            var performanceList = new List<PerformanceStatisics>();
            performanceList.Add(myStgStats);
            RecordUtil.recordToCsv(performanceList, GetType().FullName, "performance", parameters: "ShortOptionByMA", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //统计指标在console 上输出
            Console.WriteLine("--------Strategy Performance Statistics--------\n");
            Console.WriteLine(" netProfit:{0,5:F4} \n totalReturn:{1,-5:F4} \n anualReturn:{2,-5:F4} \n anualSharpe :{3,-5:F4} \n winningRate:{4,-5:F4} \n PnLRatio:{5,-5:F4} \n maxDrawDown:{6,-5:F4} \n maxProfitRatio:{7,-5:F4} \n informationRatio:{8,-5:F4} \n alpha:{9,-5:F4} \n beta:{10,-5:F4} \n averageHoldingRate:{11,-5:F4} \n", myStgStats.netProfit, myStgStats.totalReturn, myStgStats.anualReturn, myStgStats.anualSharpe, myStgStats.winningRate, myStgStats.PnLRatio, myStgStats.maxDrawDown, myStgStats.maxProfitRatio, myStgStats.informationRatio, myStgStats.alpha, myStgStats.beta, myStgStats.averageHoldingRate);
            Console.WriteLine("-----------------------------------------------\n");

            //benchmark净值
            List<double> netWorthOfBenchmark = benchmark.Select(x => x / benchmark[0]).ToList();
            line.Add("50ETF", netWorthOfBenchmark.ToArray());
            // ivix数据
            double[] iVixClose = iVix.Select(x => x.close/iVix[0].close).ToArray();
            //line.Add("iVix", iVixClose);
            string[] datestr = accountHistory.Select(a => a.time.ToString("yyyyMMdd")).ToArray();
           

            //maoheng 画图
            //Application.Run(new PLChart(line, datestr));

            //cuixun 画图
            //绘制图形的标题
            string formTitle = this.startDate.ToShortDateString() + "--" + this.endDate.ToShortDateString() + "  " + this.targetVariety + " 净值曲线"
                + "\r\n" + "\r\n" + "净利润：" + myStgStats.netProfit + "  " + "夏普率：" + myStgStats.anualSharpe + "  " + "最大回撤：" + myStgStats.maxDrawDown
                + "\r\n" + "\r\n";
            //生成图像
            PLChart plc = new PLChart(line, datestr, formTitle: formTitle);
            //运行图像
            //Application.Run(plc);
            plc.LoadForm();
            //保存图像
            plc.SaveZed(GetType().FullName, this.targetVariety, this.startDate, this.endDate, myStgStats.netProfit.ToString(), myStgStats.anualSharpe.ToString(), myStgStats.maxDrawDown.ToString());
        }
        private void tradeAssistant(ref Dictionary<string, List<KLine>> dataToday, ref Dictionary<string, MinuteSignal> signal, string code, double volume, DateTime today, DateTime now, int index)
        {

            List<OptionMinute> myData = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(code, today);
            //获取给定的期权合约的当日分钟数据
            if (dataToday.ContainsKey(code) == false)
            {
                dataToday.Add(code, myData.Cast<KLine>().ToList());
            }
            if (volume != 0)
            {
                MinuteSignal signal0 = new MinuteSignal() { code = code, volume = volume, time = now, tradingVarieties = "option", price = myData[index].open, minuteIndex = index };
                signal.Add(code, signal0);
            }

        }
    }
}
