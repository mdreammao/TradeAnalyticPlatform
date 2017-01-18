using Autofac;
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
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.Strangle
{
    public class Strangle
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        private DateTime startDate, endDate;
        //回测参数设置
        private double initialCapital = 10000;
        private double slipPoint = 0.001;
        private string targetVariety = "510050.SH";
        private double motion = 0.12;
        //构造函数
        public Strangle(int start, int end)
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
            //交易日信息
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startDate, endDate);
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
                    if (dateStructure[i] >= 40 && dateStructure[i] <= 60)
                    {
                        duration = dateStructure[i];
                        break;
                    }
                }
                
                //策略思路，当前没有持仓的时候开仓，如果有持仓就判断需不需要止盈或者换月
                if (pairs.Count()==0) //没有持仓则开仓，简单的在开盘1分钟的时候开仓
                {
                    int openIndex = 0;// 开盘1分钟开仓
                    DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), openIndex);
                    double etfPriceNow = etfData[openIndex].open;
                    //选取指定的看涨期权
                    var list = OptionUtilities.getOptionListByDate(OptionUtilities.getOptionListByStrike(OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, tradeDays[day], duration), "认购"), etfPriceNow, etfPriceNow + 0.5), Kit.ToInt_yyyyMMdd(today)).OrderBy(x => x.strike).ToList();
                    OptionInfo call = list[0];
                    //根据给定的看涨期权选取对应的看跌期权
                    OptionInfo put = OptionUtilities.getCallByPutOrPutByCall(optionInfoList, call);
                    if (call.strike!=0 && put.strike!=0) //跨式期权组合存在进行开仓
                    {
                        //获取给定的期权合约的当日分钟数据
                        List<OptionMinute> callData=Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(call.optionCode, today);
                        List<OptionMinute> putData = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(put.optionCode, today);
                        dataToday.Add(call.optionCode, callData.Cast<KLine>().ToList());
                        dataToday.Add(put.optionCode, putData.Cast<KLine>().ToList());
                        MinuteSignal openCall = new MinuteSignal() { code = call.optionCode, volume = call.contractMultiplier, time = now, tradingVarieties = "option", price = callData[openIndex].open, minuteIndex = openIndex };
                        MinuteSignal openPut = new MinuteSignal() { code = put.optionCode, volume = put.contractMultiplier, time = now, tradingVarieties = "option", price = putData[openIndex].open, minuteIndex = openIndex };
                        Console.WriteLine("开仓！call: {0}, 价格 :{1}, put: {2}, 价格: {3}",call.optionName,callData[openIndex].open,put.optionName,putData[openIndex].open);
                        signal.Add(call.optionCode, openCall);
                        signal.Add(put.optionCode,openPut);
                        StranglePair openPair = new StranglePair() {callCode=call.optionCode,putCode=put.optionCode,callPosition=1,putPosition=1,endDate=call.endDate,etfPrice= etfPriceNow,callStrike=call.strike,putStrike=put.strike,modifiedDate=now,strangleOpenPrice= callData[openIndex].open + putData[openIndex].open,closeDate=new DateTime(),closePrice=0};
                        List<StranglePair> pairList = new List<StranglePair>();
                        pairList.Add(openPair);
                        pairs.Add(now, pairList);
                    }
                    MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: openIndex);
                }
                else //逐一检查每一个配对，看看需不需要调仓或者止盈止损
                {
                    //在当日开盘时候，先判断要不要挪仓
                    foreach (var item in pairs)
                    {
                        StranglePair pair = item.Value.Last();
                        //若该组跨式组合已经关闭，则跳过统计
                        if (pair.closePrice!=0)
                        {
                            continue;
                        }
                        //若离交割超过20个交易日，则跳过统计
                        if (DateUtils.GetSpanOfTradeDays(today,pair.endDate)>=20)
                        {
                            continue;
                        }
                        int modifiedIndex = 0; //开盘第一分钟调仓。
                        DateTime now = TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]),modifiedIndex);
                        double etfPriceNow = etfData[modifiedIndex].open;
                        //写入对应call和put的数据
                       
                        //List<OptionMinute> callData = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(call.optionCode, today);
                        //List<OptionMinute> putData = Platforms.container.Resolve<OptionMinuteRepository>().fetchFromLocalCsvOrWindAndSave(put.optionCode, today);
                        if (etfPriceNow>=pair.etfPrice+motion) //向上运动超过一定幅度，则换call
                        {

                        }
                        else if (etfPriceNow<=pair.etfPrice-motion) //向下运动超过一定幅度，则换put
                        {

                        }
                        else  //简单的关闭该组合，并重新开仓平值跨式期权
                        {

                            //MinuteSignal closeCall = new MinuteSignal() { code = call.optionCode, volume = call.contractMultiplier, time = now, tradingVarieties = "option", price = callData[openIndex].open, minuteIndex = openIndex };
                            //MinuteSignal closePut = new MinuteSignal() { code = put.optionCode, volume = put.contractMultiplier, time = now, tradingVarieties = "option", price = putData[openIndex].open, minuteIndex = openIndex };
                        }

                    }

                    for (int index = 0; index < 234; index++) //遍历日内的逐个分钟(不包括最后5分钟)
                    {
                        DateTime now= TimeListUtility.IndexToMinuteDateTime(Kit.ToInt_yyyyMMdd(tradeDays[day]), index);
                        double etfPriceNow = etfData[index].open;
                        foreach (var item in pairs)
                        {
                            StranglePair pair = item.Value.Last();
                            //检查每一个跨式或者宽跨式组合，看看需不需要调整
                            
                            double etfPriceLast = pair.etfPrice;
                            
                            
                        }
                    }
                    
                }
            }


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



    }
}
