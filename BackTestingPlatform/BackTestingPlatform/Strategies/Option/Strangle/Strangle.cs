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
using BackTestingPlatform.Strategies.Option.Strangle.Model;
using BackTestingPlatform.Transaction.Minute.maoheng;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.SaveResult.Common;
using BackTestingPlatform.Utilities.SaveResult.Option;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackTestingPlatform.Strategies.Option.Strangle
{
    public class Strangle
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
        public Strangle(int start, int end)
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
            //初始化(宽)跨式组合信息
            SortedDictionary<DateTime, List<StranglePair>> pairs = new SortedDictionary<DateTime, List<StranglePair>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.initialAssets = initialCapital;
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();
            List<double> benchmark = new List<double>();
            
            //50ETF的日线数据准备，从回测期开始之前100个交易开始取
            int number = 100;
            List<StockDaily> dailyData = new List<StockDaily>();
            dailyData = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, DateUtils.PreviousTradeDay(startDate, number), endDate);
            var closePrice = dailyData.Select(x => x.close).ToArray();
            //按交易日回测
            for (int day = 0; day < tradeDays.Count(); day++)
            {
               
                benchmark.Add(closePrice[day + number]);
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
                for (int i = 0; i < dateStructure.Count(); i++)
                {
                    if (dateStructure[i] >= 40 && dateStructure[i] <= 80)
                    {
                        duration = dateStructure[i];
                        break;
                    }
                }
                for (int index = 0; index < 234; index++) //遍历日内的逐个分钟(不包括最后5分钟)
                {
                        
                    signal = new Dictionary<string, MinuteSignal>();
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), index);
                    double etfPriceNow = etfData[index].open;
                    //策略思路，当前没有持仓的时候开仓，如果有持仓就判断需不需要止盈或者换月
                    if (pairs.Count == 0 || (pairs.Count > 0 && pairs.Last().Value.Last().closePrice != 0)) //没有持仓则开仓，简单的在开盘1分钟的时候开仓
                    {
                        signal = new Dictionary<string, MinuteSignal>();
                        //今日没有合适的期权合约
                        if (duration==0)
                        {
                            continue;
                        }
                        openStrangle(ref dataToday, ref signal, ref positions, ref myAccount, ref pairs, today, index, duration);
                    }

                    StranglePair pair = pairs.Last().Value.Last();
                    double durationNow = DateUtils.GetSpanOfTradeDays(today, pair.endDate);
                    //如果有持仓，通过以下手段记录每日分钟信息
                    if ((pairs.Count > 0 && pairs.Last().Value.Last().closePrice == 0) && (dataToday.ContainsKey(pair.callCode)==false ||dataToday.ContainsKey(pair.putCode)==false))
                    {
                        tradeAssistant(ref dataToday, ref signal, pair.callCode, 0, today, now, index);
                        tradeAssistant(ref dataToday, ref signal, pair.putCode, 0, today, now, index);
                    }
                    //检查每一个跨式或者宽跨式组合，看看需不需要调整
                    if (durationNow < 10 && etfPriceNow < pair.callStrike + motion && etfPriceNow > pair.putStrike - motion) //不用调仓直接平仓
                    {
                        closeStrangle(ref dataToday, ref signal, ref positions, ref myAccount, ref pairs, today, index);
                    }
                    else if (durationNow < 20 && !(etfPriceNow < pair.callStrike + motion && etfPriceNow > pair.putStrike - motion)) //跨期调仓
                    {
                        //先进行平仓，然后开仓
                        closeStrangle(ref dataToday, ref signal, ref positions, ref myAccount, ref pairs, today, index);//平仓
                        signal = new Dictionary<string, MinuteSignal>();
                        StranglePair newPair = new StranglePair();
                        if (etfPriceNow > pair.callStrike + motion)
                        {
                            OptionInfo call = getOptionCode(duration, etfPriceNow, "认购", today);
                            OptionInfo put = getOptionCode(duration, pair.putStrike, "认沽", today);
                            if (call.strike != 0 && put.strike != 0) //开仓
                            {
                                newPair = new StranglePair() { callCode = call.optionCode, callStrike = call.strike, callPosition = call.contractMultiplier, putCode = put.optionCode, putStrike = put.strike, putPosition = put.contractMultiplier, closeDate = new DateTime(), closePrice = 0, endDate = call.endDate, etfPrice = etfPriceNow, modifiedDate = today, strangleOpenPrice = dataToday[call.optionCode][index].open + dataToday[put.optionCode][index].open };
                                modifyStrangle(ref dataToday, ref signal, ref positions, ref myAccount, ref pairs, ref newPair, today, index);
                            }
                        }
                        else if (etfPriceNow < pair.putStrike - motion)
                        {
                            OptionInfo call = getOptionCode(duration, pair.callStrike, "认购", today);
                            OptionInfo put = getOptionCode(duration, etfPriceNow, "认沽", today);
                            if (call.strike != 0 && put.strike != 0) //开仓
                            {
                                newPair = new StranglePair() { callCode = call.optionCode, callStrike = call.strike, callPosition = call.contractMultiplier, putCode = put.optionCode, putStrike = put.strike, putPosition = put.contractMultiplier, closeDate = new DateTime(), closePrice = 0, endDate = call.endDate, etfPrice = etfPriceNow, modifiedDate = today, strangleOpenPrice = dataToday[call.optionCode][index].open + dataToday[put.optionCode][index].open };
                                modifyStrangle(ref dataToday, ref signal, ref positions, ref myAccount, ref pairs, ref newPair, today, index);
                            }
                        }
                    }
                    else if (durationNow >= 20 && !(etfPriceNow < pair.callStrike + motion && etfPriceNow > pair.putStrike - motion)) //不跨期调仓
                    {

                        if (etfPriceNow > pair.callStrike + motion)
                        {
                            //认购期权向上移仓
                            OptionInfo call = getOptionCode(durationNow, etfPriceNow, "认购", today);
                            if (call.strike != 0)
                            {
                                tradeAssistant(ref dataToday, ref signal, pair.putCode, 0, today, now, index);
                                tradeAssistant(ref dataToday, ref signal, pair.callCode, -pair.callPosition, today, now, index);
                                tradeAssistant(ref dataToday, ref signal, call.optionCode, call.contractMultiplier, today, now, index);
                                pair.closeDate = now;
                                pair.closePrice = dataToday[pair.callCode][index].open + dataToday[pair.putCode][index].open;
                                StranglePair newPair = new StranglePair() { callCode = call.optionCode, callStrike = call.strike, callPosition = call.contractMultiplier, putCode = pair.putCode, putStrike = pair.putStrike, putPosition = pair.putPosition, closeDate = new DateTime(), closePrice = 0, endDate = call.endDate, etfPrice = etfPriceNow, modifiedDate = now, strangleOpenPrice = dataToday[call.optionCode][index].open + dataToday[pair.putCode][index].open };
                                pairs.Last().Value.Add(newPair);
                                MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
                            }
                        }
                        else if (etfPriceNow < pair.putStrike - motion)
                        {
                            //认沽期权向下移仓
                            OptionInfo put = getOptionCode(durationNow, etfPriceNow, "认沽", today);
                            if (put.strike != 0)
                            {
                                tradeAssistant(ref dataToday, ref signal, pair.callCode, 0, today, now, index);
                                tradeAssistant(ref dataToday, ref signal, pair.putCode, -pair.putPosition, today, now, index);
                                tradeAssistant(ref dataToday, ref signal, put.optionCode, put.contractMultiplier, today, now, index);
                                pair.closeDate = now;
                                pair.closePrice = dataToday[pair.callCode][index].open + dataToday[pair.putCode][index].open;
                                StranglePair newPair = new StranglePair() { callCode = pair.callCode, callStrike = pair.callStrike, callPosition = pair.callPosition, putCode = put.optionCode, putStrike = put.strike, putPosition = put.contractMultiplier, closeDate = new DateTime(), closePrice = 0, endDate = put.endDate, etfPrice = etfPriceNow, modifiedDate = now, strangleOpenPrice = dataToday[pair.callCode][index].open + dataToday[put.optionCode][index].open };
                                pairs.Last().Value.Add(newPair);
                                MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
                            }
                        }
                    }
                }
                if (etfData.Count > 0)
                {
                    //更新当日属性信息

                    AccountOperator.Minute.maoheng.AccountUpdatingWithMinuteBar.computeAccount(ref myAccount, positions, etfData.Last().time, etfData.Count() - 1, dataToday);

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
            RecordUtil.recordToCsv(accountHistory, GetType().FullName, "account", parameters: "strangle", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录持仓变化
            var positionStatus = OptionRecordUtil.Transfer(positions);
            RecordUtil.recordToCsv(positionStatus, GetType().FullName, "positions", parameters: "strangle", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录统计指标
            var performanceList = new List<PerformanceStatisics>();
            performanceList.Add(myStgStats);
            RecordUtil.recordToCsv(performanceList, GetType().FullName, "performance", parameters: "strangle", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
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
        /// <summary>
        /// 跨式期权调仓
        /// </summary>
        /// <param name="dataToday"></param>
        /// <param name="signal"></param>
        /// <param name="positions"></param>
        /// <param name="myAccount"></param>
        /// <param name="pairs"></param>
        /// <param name="today"></param>
        /// <param name="index"></param>
        /// <param name="duration"></param>
        private void modifyStrangle(ref Dictionary<string, List<KLine>> dataToday, ref Dictionary<string, MinuteSignal> signal, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, ref SortedDictionary<DateTime, List<StranglePair>> pairs,ref StranglePair newPair, DateTime today, int index)
        {
            DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), index);
            tradeAssistant(ref dataToday, ref signal, newPair.callCode, newPair.callPosition, today, now, index);
            tradeAssistant(ref dataToday, ref signal,newPair.putCode, newPair.putPosition, today, now, index);
            pairs.Last().Value.Add(newPair);
            MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
        }

        /// <summary>
        /// 跨式期权平仓
        /// </summary>
        /// <param name="dataToday"></param>
        /// <param name="signal"></param>
        /// <param name="positions"></param>
        /// <param name="myAccount"></param>
        /// <param name="pairs"></param>
        /// <param name="today"></param>
        /// <param name="index"></param>
        private void closeStrangle(ref Dictionary<string, List<KLine>> dataToday, ref Dictionary<string, MinuteSignal> signal, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, ref SortedDictionary<DateTime, List<StranglePair>> pairs,DateTime today, int index)
        {
            DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), index);
            double etfPriceNow = dataToday[targetVariety][index].open;
            List<StranglePair> holdPairs = pairs.Last().Value;
            StranglePair pair = holdPairs.Last();
            //关闭当前组合，为挪仓做准备
            tradeAssistant(ref dataToday, ref signal, pair.callCode, -pair.callPosition, today, now, index);
            tradeAssistant(ref dataToday, ref signal, pair.putCode, -pair.putPosition, today, now, index);
            pair.closeDate = now;
            pair.closePrice = dataToday[pair.callCode][index].open + dataToday[pair.putCode][index].open;
            MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
        }

        /// <summary>
        /// 跨式期权开仓
        /// </summary>
        /// <param name="dataToday"></param>
        /// <param name="signal"></param>
        /// <param name="positions"></param>
        /// <param name="myAccount"></param>
        /// <param name="pairs"></param>
        /// <param name="today"></param>
        /// <param name="index"></param>
        /// <param name="duration"></param>
        private void openStrangle(ref Dictionary<string, List<KLine>> dataToday, ref Dictionary<string, MinuteSignal> signal, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, ref SortedDictionary<DateTime, List<StranglePair>> pairs,DateTime today, int index,double duration)
        {
            DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), index);
            double etfPriceNow = dataToday[targetVariety][index].open;
            //选取指定的看涨期权
            var list = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购"), etfPriceNow, etfPriceNow + 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => x.strike).ToList();
            OptionInfo call = list[0];
            //根据给定的看涨期权选取对应的看跌期权
            OptionInfo put = OptionUtilities.getCallByPutOrPutByCall(optionInfoList, call);
            if (call.strike != 0 && put.strike != 0) //跨式期权组合存在进行开仓
            {
                tradeAssistant(ref dataToday, ref signal, call.optionCode, call.contractMultiplier, today, now, index);
                tradeAssistant(ref dataToday, ref signal, put.optionCode, put.contractMultiplier, today, now, index);
                StranglePair openPair = new StranglePair() { callCode = call.optionCode, putCode = put.optionCode, callPosition = call.contractMultiplier, putPosition = put.contractMultiplier, endDate = call.endDate, etfPrice = etfPriceNow, callStrike = call.strike, putStrike = put.strike, modifiedDate = now, strangleOpenPrice = dataToday[call.optionCode][index].open + dataToday[put.optionCode][index].open, closeDate = new DateTime(), closePrice = 0 };
                List<StranglePair> pairList = new List<StranglePair>();
                pairList.Add(openPair);
                pairs.Add(now, pairList);
            }
            MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: index);
        }
        private void tradeAssistant(ref Dictionary<string, List<KLine>> dataToday,ref Dictionary<string, MinuteSignal> signal,string code,double volume,DateTime today,DateTime now,int index)
        {

            List<OptionMinute> myData = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(code, today);
            //获取给定的期权合约的当日分钟数据
            if (dataToday.ContainsKey(code)==false)
            {
                dataToday.Add(code, myData.Cast<KLine>().ToList());
            }
            if (volume != 0)
            {
                MinuteSignal signal0 = new MinuteSignal() { code = code, volume = volume, time = now, tradingVarieties = "option", price = myData[index].open, minuteIndex = index };
                signal.Add(code, signal0);
            }

        }

        /// <summary>
        /// 获取对应的期权合约
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="strike"></param>
        /// <param name="etfPriceNow"></param>
        /// <param name="type"></param>
        /// <param name="today"></param>
        /// <returns></returns>
        private OptionInfo getOptionCode(double duration,double etfPriceNow,string type,DateTime today)
        {
            OptionInfo option = new OptionInfo();
            if (type=="认购")
            {
                //选取指定的看涨期权
                var list = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购"), etfPriceNow, etfPriceNow + 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => x.strike).ToList();
                if (list.Count>0)
                {
                    option = list[0];
                }
            }
            else if (type=="认沽")
            {
                //选取指定的看跌期权
                var list = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽"), etfPriceNow-0.5, etfPriceNow), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => x.strike).ToList();
                if (list.Count > 0)
                {
                    option = list[0];
                }
            }
            return option;
        }
    }
}
