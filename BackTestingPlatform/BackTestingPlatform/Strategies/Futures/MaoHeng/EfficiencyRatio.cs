using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Transaction.Minute.maoheng;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng
{
    /// <summary>
    /// Perry Kaufman在他 1995年的着作 " Smarter Trading"首次提出了效率比值 （ER）。它是一种趋势强度的衡量,用以调整市场的波动程度.它的计算方法如下 Efficiency Ratio = direction / volatility ER = （N 期间内价格总变化的绝对值）/ （N 期间内个别价格变化的绝对值） 如果个别价格变化都是正值 （或负值），那么 ER 将等于 1.0，这代表了强劲的趋势行情。然而，如果有正面和负面价格变动造成相互的抵消，代表公式中的分子将会缩小，ER 将会减少。ER 反映价格走势的一致性。ER 的所有值将都介于 0.0 ~ 1.0  另外一种计算方式为ER = （N 期间内价格总变化）/ （N 期间内个别价格变化的绝对值）此时 ER值的变化范围就会落在 -1.0 ~ 1.0 之间 , 分别代表涨势与跌势的方向 , 其中 0 代表无方向性的波动
    /// </summary>
    public class EfficiencyRatio
    {
        //回测参数设置
        private double initialCapital = 25000;
        private double optionVolume = 10000;
        private double slipPoint = 0;
        private DateTime startDate, endDate;
        private string underlying;
        private int frequency = 1;
        private int numbers = 5;
        private double longLevel = 0.8, shortLevel = -0.8;
        private List<DateTime> tradeDays = new List<DateTime>();
        private Dictionary<DateTime, int> timeList = new Dictionary<DateTime, int>();
        /// <summary>
        /// 策略的构造函数
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public EfficiencyRatio(int startDate, int endDate,string underlying,int frequency=1,int numbers=3,double longLevel=0.75,double shortLevel=-0.75)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            this.underlying = underlying;
            this.frequency = frequency;
            this.numbers = numbers;
            this.longLevel = longLevel;
            this.shortLevel = shortLevel;
            this.tradeDays = DateUtils.GetTradeDays(startDate, endDate);
        }
        private  List<FuturesMinute> getData(DateTime today,string code)
        {
            var data = Platforms.container.Resolve<FuturesMinuteRepository>().fetchFromLocalCsvOrWindAndSave(code, today);
            var data5=FreqTransferUtils.minuteToNMinutes(data, 5);
            return data5;
        }

        private double computeER(double[] prices)
        {
            double direction = 0, volatility = 0;
            direction = prices.Last() - prices.First();
            for (int i = 1; i < prices.Count(); i++)
            {
                volatility += Math.Abs(prices[i] - prices[i - 1]);
            }
            return direction / volatility;
        }
        /// <summary>
        /// 获取日期和下标对应的映射表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        private Dictionary<DateTime,int> getTimeList<T>(List<T> data) where T : KLine, new()
        {
            Dictionary<DateTime, int> timeList = new Dictionary<DateTime, int>();
            for (int i = 0; i < data.Count(); i++)
            {
                DateTime now = data[i].time;
                DateTime timeInDay = new DateTime(1, 1, 1, now.Hour, now.Minute, now.Second);
                timeList.Add(timeInDay, i);
            }
            return timeList;
        }

        public void compute()
        {
            //初始化头寸信息
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount(initialAssets: initialCapital, totalAssets: initialCapital, freeCash: initialCapital);
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();
            List<double> benchmark = new List<double>();
            double positionVolume = 0;
            double maxIncome = 0;
            for (int i = 0; i < tradeDays.Count(); i++)
            {
                DateTime today = tradeDays[i];
                var data = getData(today, underlying);
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                dataToday.Add(underlying, data.Cast<KLine>().ToList());
                
                for (int j = numbers; j < data.Count()-5; j++)
                {
                    DateTime now = data[j].time;
                    //追踪止损判断 触发止损平仓
                    if (positionVolume != 0)
                    {
                        double incomeNow = individualIncome(positions.Last().Value[underlying], data[j].open);
                        if (incomeNow>maxIncome)
                        {
                            maxIncome = incomeNow;
                        }
                        else if ((maxIncome-incomeNow)>5  || maxIncome>45) //从最高点跌下来3%，就止损
                        {
                            positionVolume = 0;
                            Console.WriteLine("追踪止损！平仓价格: {0}",data[j].open);
                            MinuteCloseAllWithBar.CloseAllPosition(dataToday, ref positions, ref myAccount, now, j, slipPoint);
                            maxIncome = 0;
                        }
                    }
                    double[] prices = new double[numbers];
                    for (int k = j-numbers; k < j; k++)
                    {
                        prices[k - (j - numbers)] = data[k].close;
                    }

                    double ER = computeER(prices);
                    if (ER>=longLevel && positionVolume==0) //多头信号,无头寸，则开多仓
                    {
                        double volume = 1;
                        MinuteSignal longSignal = new MinuteSignal() { code = underlying, volume = volume, time = now, tradingVarieties = "futures", price = data[j].open, minuteIndex = j };
                        Console.WriteLine("做多期货！多头开仓价格: {0}",data[j].open);
                        Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                        signal.Add(underlying, longSignal);
                        positionVolume += volume;
                        MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: longSignal.minuteIndex);
                    }
                    else if (ER<=shortLevel && positionVolume == 0) //空头信号，无头寸，则开空仓
                    {
                        double volume = -1;
                        MinuteSignal shortSignal = new MinuteSignal() { code = underlying, volume = volume, time = now, tradingVarieties = "futures", price = data[j].open, minuteIndex = j };
                        Console.WriteLine("做空期货！空头开仓价格: {0}",data[j].open);
                        Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                        signal.Add(underlying, shortSignal);
                        positionVolume += volume;
                        MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: shortSignal.minuteIndex);
                    }
                }
                int closeIndex = data.Count() - 5;
                if (positionVolume != 0)
                {
                    positionVolume = 0;
                    Console.WriteLine("{2}   每日收盘前强制平仓，平仓价格:{0},账户价值:{1}", data[closeIndex].open, myAccount.totalAssets, today);
                    MinuteCloseAllWithBar.CloseAllPosition(dataToday, ref positions, ref myAccount, data[closeIndex].time, closeIndex, slipPoint);
                }
            }

        }
        private double individualIncome(PositionsWithDetail position, double price)
        {
            double income = 0;
            if (position.LongPosition.volume!=0)
            {
                return (price - position.LongPosition.averagePrice);
            }
            if (position.ShortPosition.volume!=0)
            {
                return (price - position.ShortPosition.averagePrice);
            }
            return income;
        }

        private bool stopLossOrProfit(PositionsWithDetail position,double price)
        {
            bool stop = false;
            //多头止损
            if (position.LongPosition.volume>0 &&　(price-position.LongPosition.averagePrice)/ position.LongPosition.averagePrice<-0.01)
            {
                stop = true;
            }
            //多头止盈
            if (position.LongPosition.volume > 0 && (price - position.LongPosition.averagePrice) / position.LongPosition.averagePrice > 0.01)
            {
                stop = true;
            }
            //空头止损
            if (position.ShortPosition.volume < 0 && (price-position.ShortPosition.averagePrice) / position.ShortPosition.averagePrice > 0.01)
            {
                stop = true;
            }
            //空头止盈
            if (position.ShortPosition.volume < 0 && (price - position.ShortPosition.averagePrice) / position.ShortPosition.averagePrice < -0.01)
            {
                stop = true;
            }
            return stop;
        }

    }
}
