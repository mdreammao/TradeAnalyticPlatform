using Autofac;
using BackTestingPlatform.Charts;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Transaction.Minute.maoheng;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Utilities.DataApplication;
using BackTestingPlatform.Utilities.SaveResult.Common;
using BackTestingPlatform.Utilities.SaveResult.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BackTestingPlatform.AccountOperator.Minute.maoheng;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng
{
    /// <summary>
    /// Perry Kaufman在他 1995年的着作 " Smarter Trading"首次提出了效率比值 （ER）。它是一种趋势强度的衡量,用以调整市场的波动程度.它的计算方法如下 Efficiency Ratio = direction / volatility ER = （N 期间内价格总变化的绝对值）/ （N 期间内个别价格变化的绝对值） 如果个别价格变化都是正值 （或负值），那么 ER 将等于 1.0，这代表了强劲的趋势行情。然而，如果有正面和负面价格变动造成相互的抵消，代表公式中的分子将会缩小，ER 将会减少。ER 反映价格走势的一致性。ER 的所有值将都介于 0.0 ~ 1.0  另外一种计算方式为ER = （N 期间内价格总变化）/ （N 期间内个别价格变化的绝对值）此时 ER值的变化范围就会落在 -1.0 ~ 1.0 之间 , 分别代表涨势与跌势的方向 , 其中 0 代表无方向性的波动
    /// </summary>
    public class EfficiencyRatio
    {
        //回测参数设置
        private double initialCapital = 3000;
        private double slipPoint = 0;
        private DateTime startDate, endDate;
        private string underlying;
        private int frequency = 1;
        private int numbers = 5;
        private double longLevel = 0.8, shortLevel = -0.8;
        private List<DateTime> tradeDays = new List<DateTime>();
        private Dictionary<DateTime, int> timeList = new Dictionary<DateTime, int>();
        private List<NetValue> netValue = new List<NetValue>();
        /// <summary>
        /// 策略的构造函数
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="underlying"></param>
        /// <param name="frequency"></param>
        /// <param name="numbers"></param>
        /// <param name="longLevel"></param>
        /// <param name="shortLevel"></param>
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
            if (underlying.IndexOf("RB")>-1) //螺纹钢手续费为每手万一，一手乘数为10
            {
                initialCapital = 3000;
                slipPoint = initialCapital*0.0001;
            }
            else if (underlying.IndexOf("A")>-1) //大豆的手续费为每手2块钱，一手乘数为10
            {
                initialCapital = 4000;
                slipPoint = 0.2;
            }
            else if (underlying.IndexOf("M") > -1) //大豆的手续费为每手2块钱，一手乘数为10
            {
                initialCapital = 3000;
                slipPoint = 0.15;
            }
        }

        /// <summary>
        /// 从wind或本地CSV获取当天数据
        /// </summary>
        /// <param name="today">今天的日期</param>
        /// <param name="code">代码</param>
        /// <returns></returns>
        private  List<FuturesMinute> getData(DateTime today,string code)
        {
            //从本地csv 或者 wind获取数据，从wind拿到额数据会保存在本地
            List<FuturesMinute> data =KLineDataUtils.leakFilling(Platforms.container.Resolve<FuturesMinuteRepository>().fetchFromLocalCsvOrWindAndSave(code, today));
            var dataModified=FreqTransferUtils.minuteToNMinutes(data, frequency);
            return dataModified;
        }

        /// <summary>
        /// 计算ER值...................
        /// 
        ///  Efficiency Ratio = direction / volatility...ER = （N 期间内价格总变化的绝对值）/ （N 期间内个别价格变化的绝对值） 
        ///  如果个别价格变化都是正值 （或负值），那么 ER 将等于 1.0，这代表了强劲的趋势行情。
        ///  然而，如果有正面和负面价格变动造成相互的抵消，代表公式中的分子将会缩小，ER 将会减少。
        ///  ER 反映价格走势的一致性。ER 的所有值将都介于 0.0 ~ 1.0 
        /// </summary>
        /// <param name="prices"></param>
        /// <returns></returns>
        private double computeER(double[] prices)
        {
            double direction = 0, volatility = 0;
            //计算当前K线和第一根K线的收盘价的差值,即收盘价移动的相对距离
            direction = prices.Last() - prices.First();
            //计算前N根相邻K线收盘价差值的绝对值之和，即收盘价移动的绝对距离
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
            //基准衡量
            List<double> benchmark = new List<double>();
            //持仓量
            double positionVolume = 0;
            //最大收入值
            double maxIncome = 0;

            //第一层循环：所有交易日循环一遍...
            for (int i = 0; i < tradeDays.Count(); i++)
            {
                DateTime today = tradeDays[i];
                //从wind或本地CSV获取相应交易日的数据list，并转换成FuturesMinute分钟线频率
                var dataOnlyToday = getData(today, underlying);//一个交易日里有多条分钟线数据
                var data = getData(DateUtils.PreviousTradeDay(today), underlying);//前一交易日的分钟线频率数据list
                int indexStart = data.Count();
                if (indexStart==0) //前一天没数据
                {
                    indexStart = numbers;
                }
                data.AddRange(dataOnlyToday);//将当天的数据add到前一天的数据之后
                //将获取的数据，储存为KLine格式
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                dataToday.Add(underlying, data.Cast<KLine>().ToList());

                //第二层循环：只循环某当天的数据（开始的索引值为前一天数据的List.count）
                #region 第二层循环

                //这里减1：最后一个周期只平仓，不开仓
                for (int j = indexStart; j < data.Count()-1; j++)
                {
                    DateTime now = data[j].time;
                    double[] prices = new double[numbers];
                    for (int k = j - numbers; k < j; k++)
                    {
                        //导入收盘价
                        prices[k - (j - numbers)] = data[k].close;
                    }
                    //计算出ER值
                    double ER = computeER(prices);

                    # region 追踪止损判断 触发止损平仓

                    //追踪止损判断 触发止损平仓
                    if (positionVolume != 0) //头寸量不为0，额外要做的操作 
                    {
                        //计算开盘价和头寸当前价的差价
                        double incomeNow = individualIncome(positions.Last().Value[underlying], data[j].open);
                        //若当前收入大于最大收入值，则更新最大收入值
                        if (incomeNow>maxIncome)
                        {
                            maxIncome = incomeNow;
                        }
                        //若盈利回吐大于5个点 或者 最大收入大于45，则进行平仓
                        //&& ((positionVolume>0 && ER<longLevel) || (positionVolume<0 && ER>shortLevel))
                        else if ((maxIncome-incomeNow)>0.01*Math.Abs(data[j].open) || incomeNow<-0.01 * Math.Abs(data[j].open)) //从最高点跌下来3%，就止损

                        {
                            positionVolume = 0;
                            Console.WriteLine("追踪止损！平仓价格: {0}",data[j].open);
                            MinuteCloseAllWithBar.CloseAllPosition(dataToday, ref positions, ref myAccount, now, j, slipPoint);
                            maxIncome = 0;
                        }
                        //if (positionVolume>0 && ER<0 && incomeNow<-10)
                        //{
                        //    positionVolume = 0;
                        //    Console.WriteLine("信号止损！平仓价格: {0}", data[j].open);
                        //    MinuteCloseAllWithBar.CloseAllPosition(dataToday, ref positions, ref myAccount, now, j, slipPoint);
                        //    maxIncome = 0;
                        //}
                        //else if (positionVolume < 0 && ER >0 && incomeNow < -10)
                        //{
                        //    positionVolume = 0;
                        //    Console.WriteLine("信号止损！平仓价格: {0}", data[j].open);
                        //    MinuteCloseAllWithBar.CloseAllPosition(dataToday, ref positions, ref myAccount, now, j, slipPoint);
                        //    maxIncome = 0;
                        //}
                    }

                    #endregion
                    if (ER>=longLevel && positionVolume==0) //多头信号,无头寸，则开多仓

                    {
                        double volume = 1;
                        //长头寸信号
                        MinuteSignal longSignal = new MinuteSignal() { code = underlying, volume = volume, time = now, tradingVarieties = "futures", price = data[j].open, minuteIndex = j };
                        //signal保存长头寸longSignal信号
                        Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                        signal.Add(underlying, longSignal);
                        MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: longSignal.minuteIndex);
                        Console.WriteLine("做多期货！多头开仓价格: {0}", data[j].open);
                        //头寸量叠加
                        positionVolume += volume;
                        //单笔最大收益重置
                        maxIncome = 0;
                    }
                    else if (ER<=shortLevel&& positionVolume == 0) //空头信号，无头寸，则开空仓
                    {
                        double volume = -1;
                        maxIncome = 0;
                        MinuteSignal shortSignal = new MinuteSignal() { code = underlying, volume = volume, time = now, tradingVarieties = "futures", price = data[j].open, minuteIndex = j };
                        Console.WriteLine("做空期货！空头开仓价格: {0}",data[j].open);
                        Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                        signal.Add(underlying, shortSignal);
                        positionVolume += volume;
                        //分钟级交易
                        MinuteTransactionWithBar.ComputePosition(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: now, nowIndex: shortSignal.minuteIndex);
                    }
                }

                #endregion

                int closeIndex = data.Count() - 1;

                if (positionVolume != 0)
                {
                    positionVolume = 0;
                    maxIncome = 0;
                    MinuteCloseAllWithBar.CloseAllPosition(dataToday, ref positions, ref myAccount, data[closeIndex].time, closeIndex, slipPoint);
                    Console.WriteLine("{2}   每日收盘前强制平仓，平仓价格:{0},账户价值:{1}", data[closeIndex].open, myAccount.totalAssets, today);
                }
                if (data.Count>0)
                {
                    //更新当日属性信息

                    AccountOperator.Minute.maoheng.AccountUpdatingWithMinuteBar.computeAccount(ref myAccount, positions, data.Last().time, data.Count() - 1, dataToday);

                    //记录历史仓位信息
                    accountHistory.Add(new BasicAccount(myAccount.time, myAccount.totalAssets, myAccount.freeCash, myAccount.positionValue, myAccount.margin, myAccount.initialAssets));
                    benchmark.Add(data.Last().close);
                    netValue.Add(new NetValue { time = today, netvalue = myAccount.totalAssets, benchmark = data.Last().close });
                }
            }
            //策略绩效统计及输出
            PerformanceStatisics myStgStats = new PerformanceStatisics();
            //TODO:了解该函数中计算出了那些评价标准
            myStgStats = PerformanceStatisicsUtils.compute(accountHistory, positions, benchmark.ToArray());
            //画图
            Dictionary<string, double[]> line = new Dictionary<string, double[]>();
            double[] netWorth = accountHistory.Select(a => a.totalAssets / initialCapital).ToArray();
            line.Add("NetWorth", netWorth);
            string recordName = underlying.Replace(".", "_") + "_ER_"+"numbers_"+numbers.ToString()+"_frequency_"+frequency.ToString()+"_level_"+shortLevel.ToString();
            //记录净值数据
            RecordUtil.recordToCsv(accountHistory, GetType().FullName, "account", parameters: recordName, performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            RecordUtil.recordToCsv(netValue, GetType().FullName, "netvalue", parameters: recordName, performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录持仓变化
            var positionStatus = OptionRecordUtil.Transfer(positions);
            RecordUtil.recordToCsv(positionStatus, GetType().FullName, "positions", parameters: recordName, performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录统计指标
            var performanceList = new List<PerformanceStatisics>();
            performanceList.Add(myStgStats);
            RecordUtil.recordToCsv(performanceList, GetType().FullName, "performance", parameters: recordName, performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
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
        /// 计算单独头寸的收入
        /// </summary>
        /// <param name="position">传入的头寸</param>
        /// <param name="price">传入的价格</param>
        /// <returns></returns>
        private double individualIncome(PositionsWithDetail position, double price)
        {
            double income = 0;
            if (position.LongPosition.volume!=0)
            {
                return (price - position.LongPosition.averagePrice);
            }
            if (position.ShortPosition.volume!=0)
            {
                return (position.ShortPosition.averagePrice-price);
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