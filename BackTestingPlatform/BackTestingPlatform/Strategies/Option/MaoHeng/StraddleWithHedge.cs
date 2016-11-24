using Autofac;
using BackTestingPlatform.AccountOperator.Minute;
using BackTestingPlatform.Charts;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Strategies.Option.MaoHeng.Model;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
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
using System.Windows.Forms;

namespace BackTestingPlatform.Strategies.Option.MaoHeng
{
    public class StraddleWithHedge
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        //回测参数设置
        private double initialCapital = 20000;
        private double optionVolume = 10000;
        private double slipPoint = 0.001;
        private DateTime startDate, endDate;
        private List<StockDaily> etfDailyData;
        private int startIndex;
        private int step;
        private int backTestingDuration;
        private double[] etfVol;//记录50etf的历史波动率(默认为20天)
        private double[] optionVol;//记录50etf期权的隐含波动率(利用当月和下月期权插值算出20天的隐含波动率)
        private double[] epsilon;//max[E(IV-HV),0]
        private List<OptionInfo> optionInfoList;

        public StraddleWithHedge(int startDate, int endDate, int step = 25)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            backTestingDuration = DateUtils.GetSpanOfTradeDays(this.startDate, this.endDate);
            this.step = step;
            etfDailyData = getETFHistoricalDailyData();
            startIndex = etfDailyData.Count() - backTestingDuration;
            optionInfoList = Platforms.container.Resolve<OptionInfoRepository>().fetchFromLocalCsvOrWindAndSaveAndCache(1);
        }
        public void compute()
        {
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.initialAssets = initialCapital;
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();
            List<double> benchmark = new List<double>();
            //标记当日跨式组合多空信号。1表示多头，0表示无信号，-1表示空头。
            double orignalSignal = 0;
            Straddle holdingStatus = new Straddle();
            //统计历史波动率分位数,从回测期开始前一天，统计到最后一天
            double[][] fractile = new double[backTestingDuration + 1][];
            fractile = computeRollingFractile(startIndex - 1, etfDailyData.Count() - 1, 100);
            //统计隐含波动率
            computeImpv();
            //统计隐含波动率和历史波动率之差 epsilon=max[E(IV-HV),0]
            computeEpsilon();
            //按时间遍历，2015年02月09日50ETF期权上市开始,2月10日开始昨日收盘的隐含波动率数据。
            for (int i = startIndex + 1; i < startIndex + backTestingDuration; i++)
            {
                DateTime today = etfDailyData[i].time;
                Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                double fractile90Yesterday = fractile[i - 1][9]; //昨日历史波动率90分位数
                double fractile70Yesterday = fractile[i - 1][7]; //昨日历史波动率70分位数
                double fractile50Yesterday = fractile[i - 1][5]; //昨日历史波动率50分位数
                double fractile30Yesterday = fractile[i - 1][3]; //昨日历史波动率30分位数
                double volYesterday = etfVol[i - 1]; //昨日历史波动率
                double impvYesterday = optionVol[i - 1];//昨日隐含波动率
                //获取当日ATM期权合约代码
                
                double etfPrice = etfDailyData[i].close;
                double duration;
                //获取当日期限结构,选取当月合约,若当日合约到日期小于等于3天，直接开仓下月合约
                List<double> dateStructure = OptionUtilities.getDurationStructure(optionInfoList, today);
                double duration0 = dateStructure[0] <= 3 ? dateStructure[1] : dateStructure[0];
                duration = duration0;
                var call = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购").OrderBy(x => Math.Abs(x.strike - etfPrice)).Where(x => x.startDate <= today).ToList();
                var callATM = call[0];
                var callPrice = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(callATM.optionCode, today);
                // double callImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(callATM.strike, duration / 252.0, 0.04, 0, callATM.optionType, callPrice[0].close, etfPrice);
                var put = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽").OrderBy(x => Math.Abs(x.strike - callATM.strike)).ToList();
                var putATM = put[0];
                var putPrice = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(putATM.optionCode, today);
                //double putImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(putATM.strike, duration / 252.0, 0.04, 0, putATM.optionType, putPrice[0].close, etfPrice);
                //整合当日分钟线数据
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                var etfData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", today);
                dataToday.Add("510050.SH", etfData.Cast<KLine>().ToList());
                dataToday.Add(callATM.optionCode, callPrice.Cast<KLine>().ToList());
                dataToday.Add(putATM.optionCode, putPrice.Cast<KLine>().ToList());

                //策略信号处理
                //信号1
                //orignalSignal = 0;
                //if (volYesterday >= fractile70Yesterday)
                //{
                //    //卖出跨式期权
                //    orignalSignal = -1;
                //}
                //else if (impvYesterday < volYesterday)
                //{
                //    //买入跨式期权
                //    orignalSignal = 1;
                //}
                //else if (impvYesterday - volYesterday > epsilon[i - 1])
                //{
                //    //卖出跨式期权
                //    orignalSignal = -1;
                //}
                //信号2
                orignalSignal = 0;
                if (volYesterday - impvYesterday > 0)// && volYesterday <= fractile50Yesterday)
                {
                    //买入跨式期权
                    orignalSignal = 1;
                }
                else if (impvYesterday - volYesterday > epsilon[i - 1])
                {
                    //卖出跨式期权
                    orignalSignal = -1;
                }

                //指定平仓时间为开盘第一个分钟。
                DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), 0);
                Console.WriteLine("time: {0}, 昨日历史波动率: {1}, 历史波动率70分位数: {2}, 昨日隐含波动率: {3}", now, volYesterday.ToString("N"), fractile70Yesterday.ToString("N"), optionVol[i - 1].ToString("N"));
                //如果有持仓先判断持仓状态和信号方向是否相同，如果不同先平仓
                if (holdingStatus.callPosition != 0)
                {
                    if (dataToday.ContainsKey(holdingStatus.callCode) == false)
                    {
                        var callLastDay = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(holdingStatus.callCode, today);
                        dataToday.Add(holdingStatus.callCode, callLastDay.Cast<KLine>().ToList());
                    }
                    if (dataToday.ContainsKey(holdingStatus.putCode) == false)
                    {
                        var putLastDay = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(holdingStatus.putCode, today);
                        dataToday.Add(holdingStatus.putCode, putLastDay.Cast<KLine>().ToList());
                    }
                    if (holdingStatus.callPosition * orignalSignal < 0) //仓位和信号相反，强制平仓
                    {
                        Console.WriteLine("平仓！");

                        MinuteCloseAllPositonsWithSlip.closeAllPositions(dataToday, ref positions, ref myAccount, now, slipPoint);
                        holdingStatus = new Straddle();
                    }
                    if (DateUtils.GetSpanOfTradeDays(today, holdingStatus.endDate) <= 3) //有仓位无信号，判断是否移仓
                    {
                        Console.WriteLine("平仓！");
                        MinuteCloseAllPositonsWithSlip.closeAllPositions(dataToday, ref positions, ref myAccount, now, slipPoint);
                        holdingStatus = new Straddle();
                    }
                }
                //指定开仓时间为开盘第10分钟。错开开平仓的时间。
                int openIndex = 10;
                now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), openIndex);
                if (holdingStatus.callPosition == 0 && orignalSignal != 0) //无仓位有信号，开仓
                {
                    if (orignalSignal == 1) //做多跨式期权
                    {
                        MinuteSignal openSignalCall = new MinuteSignal() { code = callATM.optionCode, volume = optionVolume, time = now, tradingVarieties = "option", price = callPrice[openIndex].close, minuteIndex = openIndex };
                        MinuteSignal openSignalPut = new MinuteSignal() { code = putATM.optionCode, volume = optionVolume, time = now, tradingVarieties = "option", price = putPrice[openIndex].close, minuteIndex = openIndex };
                        Console.WriteLine("做多跨式期权！");
                        signal.Add(callATM.optionCode, openSignalCall);
                        signal.Add(putATM.optionCode, openSignalPut);
                        //变更持仓状态
                        holdingStatus.callCode = callATM.optionCode;
                        holdingStatus.putCode = putATM.optionCode;
                        holdingStatus.callPosition = optionVolume;
                        holdingStatus.putPosition = optionVolume;
                        holdingStatus.etfPrice_open = etfData[openIndex].close;
                        holdingStatus.straddlePrice_open = callPrice[openIndex].close + putPrice[openIndex].close;
                        holdingStatus.straddleOpenDate = today;
                        holdingStatus.endDate = callATM.endDate;
                        holdingStatus.strike = callATM.strike;
                    }
                    else if (orignalSignal == -1) //做空跨式期权
                    {
                        MinuteSignal openSignalCall = new MinuteSignal() { code = callATM.optionCode, volume = -optionVolume, time = now, tradingVarieties = "option", price = callPrice[openIndex].close, minuteIndex = openIndex };
                        MinuteSignal openSignalPut = new MinuteSignal() { code = putATM.optionCode, volume = -optionVolume, time = now, tradingVarieties = "option", price = putPrice[openIndex].close, minuteIndex = openIndex };
                        Console.WriteLine("做空跨式期权！");
                        signal.Add(callATM.optionCode, openSignalCall);
                        signal.Add(putATM.optionCode, openSignalPut);
                        //变更持仓状态
                        holdingStatus.callCode = callATM.optionCode;
                        holdingStatus.putCode = putATM.optionCode;
                        holdingStatus.callPosition = -optionVolume;
                        holdingStatus.putPosition = -optionVolume;
                        holdingStatus.etfPrice_open = etfData[openIndex].close;
                        holdingStatus.straddlePrice_open = callPrice[openIndex].close + putPrice[openIndex].close;
                        holdingStatus.straddleOpenDate = today;
                        holdingStatus.endDate = callATM.endDate;
                        holdingStatus.strike = callATM.strike;
                    }
                    MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, capitalVerification: false);
                    ////计算期权delta值，并用50ETF对冲
                    //var positionLast = positions.Last().Value;
                    //double etfChangeVolume = -computeOptionDelta(positionLast, holdingStatus, today, dataToday, openIndex) - holdingStatus.etfPosition;
                    //MinuteSignal openSignalETF = new MinuteSignal() { code = "510050.SH", volume = etfChangeVolume, time = now, tradingVarieties = "stock", price = dataToday["510050.SH"][openIndex].close, minuteIndex = openIndex };
                    //signal = new Dictionary<string, MinuteSignal>();
                    //signal.Add("510050.SH", openSignalETF);
                    //MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, capitalVerification: false);
                    //holdingStatus.etfPosition += etfChangeVolume;
                }
                //每日收盘前，整理持仓情况
                int thisIndex = 239;
                double delta = 0;
                var thisTime = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(today), thisIndex);
                benchmark.Add(etfData[thisIndex].close);
                if (holdingStatus.callPosition != 0)
                {
                    if (dataToday.ContainsKey(holdingStatus.callCode) == false)
                    {
                        var callLastDay = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(holdingStatus.callCode, today);
                        dataToday.Add(holdingStatus.callCode, callLastDay.Cast<KLine>().ToList());
                    }
                    if (dataToday.ContainsKey(holdingStatus.putCode) == false)
                    {
                        var putLastDay = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(holdingStatus.putCode, today);
                        dataToday.Add(holdingStatus.putCode, putLastDay.Cast<KLine>().ToList());
                    }
                    //计算期权delta值，并用50ETF对冲
                    var positionLast = positions.Last().Value;
                    delta= computeOptionDelta(positionLast, holdingStatus, today, dataToday, thisIndex);
                    double etfChangeVolume =Math.Round( -delta - holdingStatus.etfPosition);
                    MinuteSignal openSignalETF = new MinuteSignal() { code = "510050.SH", volume = etfChangeVolume, time = thisTime, tradingVarieties = "stock", price = dataToday["510050.SH"][thisIndex].close, minuteIndex = thisIndex };
                    signal = new Dictionary<string, MinuteSignal>();
                    signal.Add("510050.SH", openSignalETF);
                    MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: thisTime, capitalVerification: false);
                    holdingStatus.etfPosition += etfChangeVolume;
                   // AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, thisTime, dataToday);
                }
                
                //更新当日属性信息
                AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, thisTime, dataToday);
                //记录历史仓位信息
                accountHistory.Add(new BasicAccount(myAccount.time, myAccount.totalAssets, myAccount.freeCash, myAccount.positionValue, myAccount.margin, myAccount.initialAssets));
                //在控制台上数据每日持仓信息
                if (holdingStatus.callPosition != 0)
                {
                    Console.WriteLine("time: {0},etf: {1}, strike: {2}, position: {3}, call: {4}, put: {5}, endDate: {6}, delta: {7}, etfVolume: {8}", thisTime, etfData[thisIndex].close, holdingStatus.strike, holdingStatus.callPosition, dataToday[holdingStatus.callCode][thisIndex].close, dataToday[holdingStatus.putCode][thisIndex].close, holdingStatus.endDate,delta,holdingStatus.etfPosition);
                }
                //Console.WriteLine("time: {0}, total: {1}, cash: {2}, option: {3}, margin: {4}", thisTime, myAccount.totalAssets, myAccount.freeCash, myAccount.positionValue, myAccount.margin);
            }
            //策略绩效统计及输出
            PerformanceStatisics myStgStats = new PerformanceStatisics();
            myStgStats = PerformanceStatisicsUtils.compute(accountHistory, positions, benchmark.ToArray());
            //画图
            Dictionary<string, double[]> line = new Dictionary<string, double[]>();
            double[] netWorth = accountHistory.Select(a => a.totalAssets / initialCapital).ToArray();
            line.Add("NetWorth", netWorth);
            //记录净值数据
            RecordUtil.recordToCsv(accountHistory, GetType().FullName, "account", parameters: "straddle", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录持仓变化
            var positionStatus = OptionRecordUtil.Transfer(positions);
            RecordUtil.recordToCsv(positionStatus, GetType().FullName, "positions", parameters: "straddle", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录统计指标
            var performanceList = new List<PerformanceStatisics>();
            performanceList.Add(myStgStats);
            RecordUtil.recordToCsv(performanceList, GetType().FullName, "performance", parameters: "straddle", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
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
        /// 计算期权持仓的delta值
        /// </summary>
        /// <param name="positionLast">最后持仓状态</param>
        /// <param name="holdingStatus">记录的跨式期权情况</param>
        /// <param name="today">今日日期</param>
        /// <param name="dataToday">今日数据</param>
        /// <param name="index">交易时间对应的下标</param>
        /// <returns></returns>
        private double computeOptionDelta(Dictionary<string, PositionsWithDetail> positionLast, Straddle holdingStatus, DateTime today, Dictionary<string, List<KLine>> dataToday, int index)
        {
            double delta = 0;
            foreach (var item in positionLast)
            {
                if (item.Key == holdingStatus.callCode)
                {
                    var callDelta = ImpliedVolatilityUtilities.ComputeOptionDelta(holdingStatus.strike, DateUtils.GetSpanOfTradeDays(today, holdingStatus.endDate) / 252.0, 0.04, 0, "认购", ImpliedVolatilityUtilities.ComputeImpliedVolatility(holdingStatus.strike, DateUtils.GetSpanOfTradeDays(today, holdingStatus.endDate) / 252.0, 0.04, 0, "认购", dataToday[item.Key][index].close, dataToday["510050.SH"][index].close), dataToday["510050.SH"][index].close);
                    delta += holdingStatus.callPosition * callDelta;
                }
                if (item.Key == holdingStatus.putCode)
                {
                    var putDelta = ImpliedVolatilityUtilities.ComputeOptionDelta(holdingStatus.strike, DateUtils.GetSpanOfTradeDays(today, holdingStatus.endDate) / 252.0, 0.04, 0, "认沽", ImpliedVolatilityUtilities.ComputeImpliedVolatility(holdingStatus.strike, DateUtils.GetSpanOfTradeDays(today, holdingStatus.endDate) / 252.0, 0.04, 0, "认沽", dataToday[item.Key][index].close, dataToday["510050.SH"][index].close), dataToday["510050.SH"][index].close);
                    delta += holdingStatus.putPosition * putDelta;
                }
            }
            return delta;
        }


        private void computeEpsilon()
        {
            double[] epsilon = new double[etfDailyData.Count()];
            double[] mean = new double[etfDailyData.Count()];
            for (int i = startIndex; i < etfDailyData.Count(); i++)
            {
                mean[i] = (mean[i - 1] * (i - startIndex) + (optionVol[i] - etfVol[i])) / (i - startIndex + 1);
                if (mean[i] > 0)
                {
                    epsilon[i] = mean[i];
                }
            }
            this.epsilon = epsilon;
        }
        private void computeImpv()
        {
            optionVol = new double[etfDailyData.Count()];
            for (int i = startIndex; i < etfDailyData.Count(); i++)
            {
                DateTime today = etfDailyData[i].time;
                double etfPrice = etfDailyData[i].close;
                double volThisMonth;
                double volNextMonth;
                double duration;
                //获取当日期限结构,选取当月合约
                List<double> dateStructure = OptionUtilities.getDurationStructure(optionInfoList, today);
                double duration0 = dateStructure[0] == 0 ? dateStructure[1] : dateStructure[0];
                duration = duration0;
                var call = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购").OrderBy(x => Math.Abs(x.strike - etfPrice)).Where(x => x.startDate <= today).ToList();
                var callATM = call[0];
                var callPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(callATM.optionCode, today, today);
                double callImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(callATM.strike, duration / 252.0, 0.04, 0, callATM.optionType, callPrice[0].close, etfPrice);
                var put = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽").OrderBy(x => Math.Abs(x.strike - callATM.strike)).ToList();
                var putATM = put[0];
                var putPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(putATM.optionCode, today, today);
                double putImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(putATM.strike, duration / 252.0, 0.04, 0, putATM.optionType, putPrice[0].close, etfPrice);
                if (callImpv * putImpv == 0)
                {
                    volThisMonth = callImpv + putImpv;
                }
                else
                {
                    volThisMonth = (callImpv + putImpv) / 2;
                }
                //获取当日期限结构,选取下月合约,若下月合约不存在，就获取季月合约
                double duration1 = dateStructure[0] == 0 ? dateStructure[2] : dateStructure[1];
                duration = duration1;
                call = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购").OrderBy(x => Math.Abs(x.strike - etfPrice)).Where(x => x.startDate <= today).ToList();
                callATM = call[0];
                callPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(callATM.optionCode, today, today);
                callImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(callATM.strike, duration / 252.0, 0.04, 0, callATM.optionType, callPrice[0].close, etfPrice);
                put = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽").OrderBy(x => Math.Abs(x.strike - callATM.strike)).ToList();
                putATM = put[0];
                putPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(putATM.optionCode, today, today);
                putImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(putATM.strike, duration / 252.0, 0.04, 0, putATM.optionType, putPrice[0].close, etfPrice);
                if (callImpv * putImpv == 0)
                {
                    volNextMonth = callImpv + putImpv;
                }
                else
                {
                    volNextMonth = (callImpv + putImpv) / 2;
                }
                if (duration0 >= step)
                {
                    optionVol[i] = Math.Sqrt(step / duration0) * volThisMonth;
                }
                else if ((duration0 < step && duration1 > step))
                {
                    optionVol[i] = Math.Sqrt((duration1 - step) / (duration1 - duration0)) * volThisMonth + Math.Sqrt((step - duration0) / (duration1 - duration0)) * volNextMonth;
                }
                else if (duration1 <= step)
                {
                    optionVol[i] = volNextMonth;
                }
            }
        }
        private List<StockDaily> getETFHistoricalDailyData()
        {
            return Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", Kit.ToDate(20130101), endDate);
        }


        private double[][] computeRollingFractile(int start, int end, int period)
        {
            double[][] disArr = new double[etfDailyData.Count()][];
            //获取前复权的价格
            double[] etfPrice = new double[etfDailyData.Count()];
            for (int i = 0; i < etfDailyData.Count(); i++)
            {
                etfPrice[i] = etfDailyData[i].close * etfDailyData[i].adjustFactor / etfDailyData.Last().adjustFactor;
            }
            //获取ETF每日年化波动率
            double[] etfVol = new double[etfDailyData.Count()];
            etfVol = Volatility.HVYearly(etfPrice, step);
            this.etfVol = etfVol;
            //统计每日波动率分位数
            List<double> volList = new List<double>();
            for (int i = start; i < etfPrice.Count(); i++)
            {
                //按周期向前推算历史波动率
                volList = etfVol.ToList().GetRange(start - period + 1, period).OrderBy(x => x).ToList();
                if (i >= start)
                {
                    int L = volList.Count() - 1;
                    disArr[i] = new double[11];
                    disArr[i][0] = volList[0];
                    disArr[i][1] = volList[(int)Math.Ceiling(L * 0.1)];
                    disArr[i][2] = volList[(int)Math.Ceiling(L * 0.2)];
                    disArr[i][3] = volList[(int)Math.Ceiling(L * 0.3)];
                    disArr[i][4] = volList[(int)Math.Ceiling(L * 0.4)];
                    disArr[i][5] = volList[(int)Math.Ceiling(L * 0.5)];
                    disArr[i][6] = volList[(int)Math.Ceiling(L * 0.6)];
                    disArr[i][7] = volList[(int)Math.Ceiling(L * 0.7)];
                    disArr[i][8] = volList[(int)Math.Ceiling(L * 0.8)];
                    disArr[i][9] = volList[(int)Math.Ceiling(L * 0.9)];
                    disArr[i][10] = volList[L];
                }
            }
            return disArr;
        }

        /// <summary>
        /// 计算历史波动率的分位数
        /// </summary>
        /// <returns></returns>
        private double[][] computeFractile(int start, int end)
        {

            double[][] disArr = new double[etfDailyData.Count()][];
            //获取前复权的价格
            double[] etfPrice = new double[etfDailyData.Count()];
            for (int i = 0; i < etfDailyData.Count(); i++)
            {
                etfPrice[i] = etfDailyData[i].close * etfDailyData[i].adjustFactor / etfDailyData.Last().adjustFactor;
            }
            //获取ETF每日年化波动率
            double[] etfVol = new double[etfDailyData.Count()];
            etfVol = Volatility.HVYearly(etfPrice, step);
            this.etfVol = etfVol;
            //统计每日波动率分位数
            List<double> volList = new List<double>();
            for (int i = 1; i < etfPrice.Count(); i++)
            {
                //按循序依次向数组中插入波动率
                if (volList.Count() == 0)
                {
                    volList.Add(etfVol[i]);
                }
                else
                {
                    if (etfVol[i] < volList[0])
                    {
                        volList.Insert(0, etfVol[i]);
                    }
                    else if (etfVol[i] > volList.Last())
                    {
                        volList.Insert(volList.Count(), etfVol[i]);
                    }
                    else
                    {
                        for (int j = 1; j < volList.Count() - 1; j++)
                        {
                            if (etfVol[i] > volList[j - 1] && etfVol[i] <= volList[j])
                            {
                                volList.Insert(j, etfVol[i]);
                                continue;
                            }
                        }
                    }
                }
                if (i >= start)
                {
                    int L = volList.Count() - 1;
                    disArr[i] = new double[11];
                    disArr[i][0] = volList[0];
                    disArr[i][1] = volList[(int)Math.Ceiling(L * 0.1)];
                    disArr[i][2] = volList[(int)Math.Ceiling(L * 0.2)];
                    disArr[i][3] = volList[(int)Math.Ceiling(L * 0.3)];
                    disArr[i][4] = volList[(int)Math.Ceiling(L * 0.4)];
                    disArr[i][5] = volList[(int)Math.Ceiling(L * 0.5)];
                    disArr[i][6] = volList[(int)Math.Ceiling(L * 0.6)];
                    disArr[i][7] = volList[(int)Math.Ceiling(L * 0.7)];
                    disArr[i][8] = volList[(int)Math.Ceiling(L * 0.8)];
                    disArr[i][9] = volList[(int)Math.Ceiling(L * 0.9)];
                    disArr[i][10] = volList[L];
                }
            }
            return disArr;
        }
    }
}

