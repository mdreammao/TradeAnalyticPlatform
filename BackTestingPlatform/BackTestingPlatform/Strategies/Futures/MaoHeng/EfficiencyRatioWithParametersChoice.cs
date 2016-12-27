using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Strategies.Futures.MaoHeng.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Utilities.DataApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng
{
    /// <summary>
    /// Perry Kaufman在他 1995年的着作 " Smarter Trading"首次提出了效率比值 （ER）。它是一种趋势强度的衡量,用以调整市场的波动程度.它的计算方法如下 Efficiency Ratio = direction / volatility ER = （N 期间内价格总变化的绝对值）/ （N 期间内个别价格变化的绝对值） 如果个别价格变化都是正值 （或负值），那么 ER 将等于 1.0，这代表了强劲的趋势行情。然而，如果有正面和负面价格变动造成相互的抵消，代表公式中的分子将会缩小，ER 将会减少。ER 反映价格走势的一致性。ER 的所有值将都介于 0.0 ~ 1.0  另外一种计算方式为ER = （N 期间内价格总变化）/ （N 期间内个别价格变化的绝对值）此时 ER值的变化范围就会落在 -1.0 ~ 1.0 之间 , 分别代表涨势与跌势的方向 , 其中 0 代表无方向性的波动
    /// 选用滚动筛选参数的方法，举例来说，可以按6个月最优选出下个月的参数。
    /// </summary>
    public class EfficiencyRatioWithParametersChoice
    {
        //回测参数设置
        private double initialCapital = 3000;
        private double slipPoint = 0;
        private DateTime startDate, endDate;
        private string underlying;
        private double lossPercent;
        private double ERRatio;
        private int frequency;
        private int numbers;
        private List<DateTime> tradeDays = new List<DateTime>();
       //private Dictionary<ParameterPairs, double> result = new Dictionary<ParameterPairs, double>();
        //每种参数组合，在所有交易日的盈利结果
        private Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> newResult = new Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>>();
        //按交易日选取出对应的参数组合
        public Dictionary<DateTime, FourParameterPairs> parameters = new Dictionary<DateTime, FourParameterPairs>();
        private Dictionary<DateTime, int> tradeIndexStart = new Dictionary<DateTime, int>();
        private Dictionary<DateTime, int> timeList = new Dictionary<DateTime, int>();

        public EfficiencyRatioWithParametersChoice(int startDate, int endDate, string underlying,int choicePeriod,int serviceLife)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            this.underlying = underlying;
            this.tradeDays = DateUtils.GetTradeDays(startDate, endDate);
            if (underlying.IndexOf("RB") > -1) //螺纹钢手续费为每手万一，一手乘数为10
            {
                initialCapital = 3000;
                slipPoint = initialCapital * 0.0001;
            }
            else if (underlying.IndexOf("A") > -1 && underlying.IndexOf("AU") < 0) //大豆的手续费为每手2块钱，一手乘数为10
            {
                initialCapital = 4000;
                slipPoint = 0.2;
            }
            else if (underlying.IndexOf("M") > -1) //大豆的手续费为每手2块钱，一手乘数为10
            {
                initialCapital = 3000;
                slipPoint = 0.15;
            }
            else if (underlying.IndexOf("AU") > -1)
            {
                initialCapital = 300;
                slipPoint = 0.02;
            }
            else if (underlying.IndexOf("NI") > -1)
            {
                initialCapital = 12000;
                slipPoint = 1;
            }
            computeParameters();
            parameters = chooseParameters(newResult, this.startDate, this.endDate, choicePeriod, serviceLife);
        }

        private void computeParameters()

        {
            List<FuturesMinute> data = new List<FuturesMinute>();
            //获取回测期之间的三日日数据
            data = getData(DateUtils.PreviousTradeDay(tradeDays[0],3), underlying);
            data.AddRange(getData(DateUtils.PreviousTradeDay(tradeDays[0], 2), underlying));
            data.AddRange(getData(DateUtils.PreviousTradeDay(tradeDays[0], 1), underlying));
            //逐日获取K线数据（频率为1分钟）
            for (int i = 0; i < tradeDays.Count(); i++)
            {
                var data0 = getData(tradeDays[i], underlying);
                data.AddRange(data0);
            }
            //var dataModified = FreqTransferUtils.minuteToNMinutes(data, frequency);
            //按交易日逐日计算，每日遍历所有的参数，结果记入字典结构的变量中
            ParameterPairs pairs = new ParameterPairs();

            int[] frequencySet = new int[5] { 5, 10, 15, 20, 30 };
            int[] numbersSet = new int[6] { 3, 4, 5, 6, 8, 10 };
            double[] lossPercentSet = new double[6] {0.000625,0.00125, 0.0025,0.005, 0.01, 0.015 };
            double[] ERRatioSet = new double[10] { 0.5, 0.55,0.6, 0.65,0.7,0.75, 0.8,0.85, 0.9,0.95 };

//#if DEBUG
//            int[] frequencySet = new int[1] { 5};
//            int[] numbersSet = new int[1] { 5 };
//            double[] lossPercentSet = new double[1] { 0.005};
//            double[] ERRatioSet = new double[1] { 0.7};
//#endif


            foreach (var fre in frequencySet)
            {
                frequency = fre; //给定K线周期
                pairs.frequency = frequency;
                var dataModified = FreqTransferUtils.minuteToNMinutes(data, frequency);
                foreach (var num in numbersSet)
                {
                    numbers = num; //给定前推的K线数量
                    pairs.numbers = numbers;
                    foreach (var loss in lossPercentSet)
                    {
                        lossPercent = loss; //给定追踪止损的参数
                        pairs.lossPercent = lossPercent;
                        foreach (var er in ERRatioSet)
                        {
                            ERRatio = er; //给定ER比例的参数
                            pairs.ERRatio = ERRatio;
                            double profitInDay = 0;
                            double positionVolume = 0;
                            double openPrice = 0;
                            double maxIncomeIndividual = 0;
                            Console.WriteLine("开始回测参数，K线：{0},回望时间：{1}，ER值：{2}，追踪止损：{3}", pairs.frequency, pairs.numbers, pairs.ERRatio, pairs.lossPercent);

                            //[新版]记录该组参数对应的, 所有交易日的收益
                            FourParameterPairs newPairs0 = new FourParameterPairs
                            {
                                ERRatio = pairs.ERRatio,
                                frequency = pairs.frequency,
                                lossPercent = pairs.lossPercent,
                                numbers = pairs.numbers
                            };

                            //用来记录同一套策略情况下，不同交易日的盈利情况
                            SortedDictionary<DateTime, double> sortedDic0 = new SortedDictionary<DateTime, double>();

                            for (int i = 0; i < dataModified.Count(); i++) //开始按日期遍历
                            {
                                var now = dataModified[i];
                                //在5分钟K线数据表dataModified中，找到首个交易日 tradeDays[0]开始位置对应的index
                                if (now.tradeday < tradeDays[0])
                                {
                                    continue;
                                }
                                pairs.tradeday = now.tradeday;
                                //当日最后一根K线，进入结算。 
                                if (i==dataModified.Count()-1 || (i+1 < dataModified.Count() && dataModified[i+1].tradeday>now.tradeday)) 
                                {
                                    //强制平仓
                                    if (positionVolume!=0)
                                    {
                                        //减去2倍的滑点，是因为买入和卖出均有手续费
                                        profitInDay += positionVolume * (now.open - openPrice)-2*slipPoint;
                                     //   Console.WriteLine("时间：{0}，价格：{1}, volume：0", dataModified[i].time, now.open);
                                    }

                                    //记录该组参数当日收益
                                    sortedDic0.Add(pairs.tradeday, profitInDay);

                                    //重置数据
                                    profitInDay = 0;
                                    positionVolume = 0;
                                    maxIncomeIndividual = 0;
                                }
                                else
                                {
                                    double[] prices = new double[numbers];
                                    for (int k = i - numbers; k < i; k++)
                                    {
                                        //导入收盘价
                                        prices[k - (i - numbers)] = dataModified[k].close;
                                    }
                                    //计算出ER值
                                    double ER = computeER(prices);
                                    if (positionVolume==0) //持空仓
                                    {
                                        if (ER>ERRatio && now.open>now.low) //开多仓,且能够开仓
                                        {
                                            openPrice = now.open;
                                            positionVolume = 1;
                                           // Console.WriteLine("时间：{0}，价格：{1}, volume：1", dataModified[i].time, now.open);
                                        }
                                        if (ER<-ERRatio && now.open<now.high) //开空仓
                                        {
                                            openPrice = now.open;
                                            positionVolume = -1;
                                           // Console.WriteLine("时间：{0}，价格：{1}, volume：-1", dataModified[i].time, now.open);
                                        }
                                    }
                                    else if (positionVolume==1) //持多仓
                                    {
                                        if ((now.open-openPrice)>maxIncomeIndividual)
                                        {
                                            maxIncomeIndividual = now.open - openPrice;
                                        }
                                        //追踪止损，强制平仓
                                        else if (((now.open - openPrice)-maxIncomeIndividual)<-lossPercent*now.open) 
                                        {
                                            profitInDay += now.open - openPrice-2*slipPoint;
                                            positionVolume = 0;
                                            maxIncomeIndividual = 0;
                                           // Console.WriteLine("时间：{0}，价格：{1}, volume：0", dataModified[i].time, now.open);
                                        }
                                    }
                                    else if (positionVolume==-1) //持空仓
                                    {
                                        if ((openPrice - now.open) > maxIncomeIndividual)
                                        {
                                            maxIncomeIndividual = (openPrice - now.open);
                                        }
                                        else if (((openPrice - now.open) - maxIncomeIndividual) < -lossPercent * now.open)
                                        {
                                            profitInDay += openPrice - now.open - 2 * slipPoint;
                                            positionVolume = 0;
                                            maxIncomeIndividual = 0;
                                           // Console.WriteLine("时间：{0}，价格：{1}, volume：0", dataModified[i].time, now.open);
                                        }
                                    }
                                }
                            }
                            //写入每一套参数，对应的所有交易日的收益情况
                            newResult.Add(newPairs0, sortedDic0);
                        }
                    }
                }                
            }
        }

        private Dictionary<DateTime, FourParameterPairs> chooseParameters(Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> result, DateTime startDate, DateTime endDate, int choicePeriod, int serviceLife)
        {
            Dictionary<DateTime, FourParameterPairs> paras = new Dictionary<DateTime, FourParameterPairs>();
            DateTime start = startDate;
            DateTime end =DateUtils.NextTradeDay(start,choicePeriod-1);
            DateTime paraStart = DateUtils.NextTradeDay(start, choicePeriod);
            DateTime paraEnd = DateUtils.NextTradeDay(start, choicePeriod + serviceLife - 1);
            if (end>endDate)
            {
                end = endDate;
                paraStart = start;
                paraEnd =end;
            }
            double marks = 0;
            FourParameterPairs bestPara = new FourParameterPairs();
            while (end<=endDate)
            {
                foreach (var item in result)
                {
                    double mark0 = getMarks(result, item.Key, start, end);
                    if (mark0>marks)
                    {
                        marks = mark0;
                        bestPara = item.Key;
                    }
                }

                if (paras.Count()==0)
                {
                    List<DateTime> dates0 = DateUtils.GetTradeDays(start, end);
                    foreach (var item in dates0)
                    {
                        paras.Add(item, bestPara);
                    }
                }
                List<DateTime> dates= DateUtils.GetTradeDays(paraStart,paraEnd);
                foreach (var item in dates)
                {
                    paras.Add(item, bestPara);
                }
                //初始化
                start= DateUtils.NextTradeDay(start, serviceLife);
                end= DateUtils.NextTradeDay(end, serviceLife);
                paraStart = DateUtils.NextTradeDay(paraStart, serviceLife);
                paraEnd = DateUtils.NextTradeDay(paraEnd, serviceLife);
                marks = 0;
                bestPara = new FourParameterPairs();
                if (end>=endDate)
                {
                    break;
                }
            }
            return paras;
        }
        private double getMarks(Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> result, FourParameterPairs pair,DateTime startDate,DateTime endDate)
        {
            List<double> netvalue = new List<double>();
            List<double> returnRatio = new List<double>();
            netvalue.Add(1);
            double total = initialCapital;
            foreach (var item in result)
            {
                if (item.Key.ERRatio==pair.ERRatio && item.Key.frequency==pair.frequency && item.Key.lossPercent==pair.lossPercent && item.Key.numbers==pair.numbers)
                {
                    foreach (var num in item.Value)
                    {
                        if (num.Key>=startDate && num.Key<=endDate)
                        {
                            total += num.Value;
                            netvalue.Add(total / initialCapital);
                            returnRatio.Add(num.Value / initialCapital);
                        }
                    }
                    break;
                }
            }
            double MDD = Math.Round(PerformanceStatisicsUtils.computeMaxDrawDown(netvalue), 4);
            double sum = 0, squareSum = 0 ;
            for (int i = 0; i < returnRatio.Count(); i++)
            {
                sum += returnRatio[i];
                squareSum += Math.Pow(returnRatio[i], 2);
            }
            double average = sum / returnRatio.Count();
            double std = Math.Sqrt(squareSum / returnRatio.Count() - average * average);
            double sharpe = average * 252 / (std * Math.Sqrt(252));
            double calmar = average * 252 / MDD;
            return 0.5*sharpe+0.5*calmar;
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
        /// 从wind或本地CSV获取当天数据
        /// </summary>
        /// <param name="today">今天的日期</param>
        /// <param name="code">代码</param>
        /// <returns></returns>
        private List<FuturesMinute> getData(DateTime today, string code)
        {
            //从本地csv 或者 wind获取数据，从wind拿到额数据会保存在本地
            List<FuturesMinute> data = KLineDataUtils.leakFilling(Platforms.container.Resolve<FuturesMinuteRepository>().fetchFromLocalCsvOrWindAndSave(code, today));
            return data;
        }

    }
}
