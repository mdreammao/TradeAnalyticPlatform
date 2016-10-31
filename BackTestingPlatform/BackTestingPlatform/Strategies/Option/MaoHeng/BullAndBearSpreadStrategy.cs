using Autofac;
using BackTestingPlatform.AccountOperator.Minute;
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
using BackTestingPlatform.Transaction;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.MaoHeng
{
    public class BullAndBearSpreadStrategy
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startDate, endDate;
        //回测参数设置
        private double initialCapital = 10000000;
        private double slipPoint = 0.000;
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
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();

            ///数据准备
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
                var today = tradeDays[day];
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
                if (ema7[day+number-1]>ema50[day+number-1]) // EMA7日线上传50日线，开牛市价差
                {
                    //取出指定日期
                    double lastETFPrice = dailyData[number + day - 1].close;
                    var etfData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(targetVariety, tradeDays[day]);
                    Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                    dataToday.Add(targetVariety, etfData.Cast<KLine>().ToList());
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), 0);
                    //MinuteSignal openSignal = new MinuteSignal() { code = targetVariety, volume = 10000, time = now, tradingVarieties = "stock", price =averagePrice, minuteIndex = day };
                    //signal.Add(targetVariety, openSignal);
                    //选取指定的看涨期权
                    var list =OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, tradeDays[day], duration),"认购"),lastETFPrice,lastETFPrice+0.5);
                    //如果可以构成看涨期权牛市价差，就开仓
                    if (list.Count()>=2)
                    {
                        var option1 = list[0];
                        var option2 = list[list.Count() - 1];
                        var option1Data = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(option1.optionCode, today);
                        var option2Data = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(option2.optionCode, today);
                        MinuteSignal openSignal1 = new MinuteSignal() { code = option1.optionCode, volume = 10000, time = now, tradingVarieties = "option", price =option1Data[0].close , minuteIndex = 0 };
                        MinuteSignal openSignal2 = new MinuteSignal() { code = option2.optionCode, volume = -10000, time = now, tradingVarieties = "option", price = option2Data[0].close, minuteIndex = 0 };
                        signal.Add(option1.optionCode, openSignal1);
                        signal.Add(option2.optionCode, openSignal2);
                    }
                    MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now);
                }
                // MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now);
            }
        }
    }
}
