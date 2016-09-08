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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option
{
    public class OptionSample4
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startdate, endDate;
        public OptionSample4(int start, int end)
        {
            startdate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
        }

        public void compute()
        {
            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startdate), Kit.ToInt_yyyyMMdd(endDate));
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            var OptionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
            Caches.put("OptionInfo", OptionInfoList);
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);
            //var ETFDaily = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", Kit.ToDate(20150101),Kit.ToDate(20160731));
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();
            foreach (var day in tradeDays)
            {
                Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
                var list = OptionUtilities.getOptionListByDate(OptionInfoList, Kit.ToInt_yyyyMMdd(day));
                List<DateTime> durationArr = OptionUtilities.getEnddateListByAscending(list);
                var ETFtoday = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", day);
                data.Add("510050.SH", ETFtoday.Cast<KLine>().ToList());
                foreach (var info in list)
                {
                    string IHCode = OptionUtilities.getCorrespondingIHCode(info, Kit.ToInt_yyyyMMdd(day));
                    var repoOption = Platforms.container.Resolve<OptionMinuteRepository>();
                    var optionToday = repoOption.fetchFromLocalCsvOrWindAndSave(info.optionCode, day);
                    data.Add(info.optionCode, optionToday.Cast<KLine>().ToList());
                }
                int index = 0;
                //初始化position及Account信息
                SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
                BasicAccount myAccount = new BasicAccount();
                while (index < 240)
                {
                    int nextIndex = index + 1;
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(day), index);
                    Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                    double etfPrice = ETFtoday[index].close;
                    List<double> strikeTodayArr = OptionUtilities.getStrikeListByAscending(list).OrderBy(x => Math.Abs(x - etfPrice)).ToList();
                    try
                    {
                        //持仓查询，先平后开
                        //若当前有持仓 且 允许平仓
                        //是否是空仓,若position中所有品种volum都为0，则说明是空仓     
                        bool isEmptyPosition = positions.Count != 0 ? positions[positions.Keys.Last()].Values.Sum(x => Math.Abs(x.volume)) == 0 : true;

                        if ((positions.Count != 0 && !isEmptyPosition) && tradingOn)
                        {
                            //平仓条件
                            //（1）若当天为交割日或回测结束日，平仓，且关闭开仓开关，次日才能开仓；
                            //（2）若closingOn为false，平仓；
                            //（3）检查持仓期权是否为平价期权，若否，清掉当前头寸并建立新的持仓；
                            //--------------------------------------------------------------------
                            //（1）若当天为交割日或回测结束日，平仓，且关闭开仓开关，次日才能开仓；
                            //（2）若closingOn为false，平仓；
                            //取出当前持仓期权的strike
                            double strikePriceOfPositions = optionInfoList[optionInfoList.FindIndex(a => a.optionCode == positions[positions.Keys.Last()].Values.First().code)].strike;
                            bool isParPriceOption = strikePriceOfPositions == strikeTodayArr[0];
                            //--------------------------------------------------------------------
                            if (!isEmptyPosition && (isExpiredDay || isLastDayOfBackTesting || closingOn == false))
                            {
                                //全部平仓
                                DateTime next = MinuteCloseAllPositonsWithSlip.closeAllPositions(data, ref positions, ref myAccount, now: now, slipPoint: slipPoint);
                                //当天不可再开仓
                                openingOn = false;
                            }
                            //（3）检查持仓期权是否为平价期权，若否，清掉当前头寸并建立新的持仓；
                            else if (!isEmptyPosition && !isParPriceOption)
                            {
                                //全部平仓
                                DateTime next = MinuteCloseAllPositonsWithSlip.closeAllPositions(data, ref positions, ref myAccount, now: now, slipPoint: slipPoint);
                                //当天不可再开仓
                                openingOn = false;
                            }
                        }
                        //若当前无持仓 且 允许开仓 
                        //若当前为交割日，则不开仓
                        if (isExpiredDay == true)
                            openingOn = false;
                        else if ((positions.Count == 0 || isEmptyPosition) && openingOn && tradingOn)
                        {
                            //标的池构建
                            //选择目标期权品种放入标的池：
                            //四个头寸（1）short当月平价认购（2）short当月平价认沽（3）long下月平价认购（4）long下月平价认沽
                            OptionInfo callCandidateFront = OptionUtilities.getSpecifiedOption(list, endDate[0], "认购", strikeTodayArr[0])[0];
                            OptionInfo putCandidateFront = OptionUtilities.getSpecifiedOption(list, endDate[0], "认沽", strikeTodayArr[0])[0];
                            OptionInfo callCandidateNext = OptionUtilities.getSpecifiedOption(list, endDate[1], "认购", strikeTodayArr[0])[0];
                            OptionInfo putCandidateNext = OptionUtilities.getSpecifiedOption(list, endDate[1], "认沽", strikeTodayArr[0])[0];

                            //检查四个标的strike是否相同，若相同则开仓，若不相同是，说明下月平价期权尚未挂出，则continue
                            bool isSameStrike = callCandidateFront.strike == callCandidateFront.strike;
                            //生成开仓信号
                            if (isSameStrike)
                            {
                                //查询可用资金
                                double nowFreeCash = myAccount.freeCash;
                                //计算每个头寸的建仓量,原则：尽量使各头寸等金额
                                double openVolumeOfCallFront = Math.Floor(nowFreeCash / 4 / data[callCandidateFront.optionCode][index].close / optionContractTimes) * optionContractTimes;
                                double openVolumeOfPutFront = Math.Floor(nowFreeCash / 4 / data[putCandidateFront.optionCode][index].close / optionContractTimes) * optionContractTimes;
                                double openVolumeOfCallNext = Math.Floor(nowFreeCash / 4 / data[callCandidateNext.optionCode][index].close / optionContractTimes) * optionContractTimes;
                                double openVolumeOfPutNext = Math.Floor(nowFreeCash / 4 / data[putCandidateNext.optionCode][index].close / optionContractTimes) * optionContractTimes;

                                MinuteSignal callFront = new MinuteSignal() { code = callCandidateFront.optionCode, volume = -openVolumeOfCallFront, time = now, tradingVarieties = "option", price = data[callCandidateFront.optionCode][index].close, minuteIndex = index };
                                MinuteSignal putFront = new MinuteSignal() { code = putCandidateFront.optionCode, volume = -openVolumeOfPutFront, time = now, tradingVarieties = "option", price = data[putCandidateFront.optionCode][index].close, minuteIndex = index };
                                MinuteSignal callNext = new MinuteSignal() { code = callCandidateNext.optionCode, volume = openVolumeOfCallNext, time = now, tradingVarieties = "option", price = data[callCandidateNext.optionCode][index].close, minuteIndex = index };
                                MinuteSignal putNext = new MinuteSignal() { code = putCandidateNext.optionCode, volume = openVolumeOfPutNext, time = now, tradingVarieties = "option", price = data[putCandidateNext.optionCode][index].close, minuteIndex = index };
                                signal.Add(callFront.code, callFront);
                                signal.Add(putFront.code, putFront);
                                signal.Add(callNext.code, callNext);
                                signal.Add(putNext.code, putNext);
                                DateTime next = MinuteTransactionWithSlip2.computeMinutePositions2(signal, data, ref positions, ref myAccount, slipPoint: slipPoint, now: now);
                                nextIndex = Math.Max(nextIndex, TimeListUtility.MinuteToIndex(next));
                                //账户信息更新
                                if (positions.Count != 0)
                                    AccountUpdating.computeAccountUpdating(ref myAccount, ref positions, now, ref data);

                            }
                        }
                        //账户信息更新
                        AccountUpdating.computeAccountUpdating(ref myAccount, positions[positions.Keys.Last()], now, ref data);
                    }
                    catch (Exception e)
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

            //将accountHistory输出到csv
            /*
            var resultPath = ConfigurationManager.AppSettings["CacheData.RootPath"];
            FileStream fs = new FileStream(resultPath, FileMode.Append);
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            //    File.WriteAllLines(@"D:\PAT\xx.txt", lines, Encoding.Default);
            
                foreach (var account in accountHistory)
                    File.WriteAllLines(@"D:\xx.csv", account.totalAssets, Encoding.Default);

                sw.Close();
                fs.Close();
                */
            foreach (var account in accountHistory)
                Console.WriteLine("time:{0},netWorth:{1,8:F3}\n", account.time, account.totalAssets / initialCapital);

            Console.ReadKey();
        }
    }
}
