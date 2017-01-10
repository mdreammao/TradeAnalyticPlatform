using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Strategies.Futures.MaoHeng.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Utilities.DataApplication;
using BackTestingPlatform.Utilities.SaveResult.Common;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng
{
    public class ERParametersChoice
    {
        //回测参数设置
        private double initialCapital = 3000;
        private double slipPoint = 0;
        private DateTime startDate, endDate;
        private string underlying;
        private double lossPercent;
        private double longER;
        private double shortER;
        private int frequency;
        private int numbers;
        private List<DateTime> tradeDays = new List<DateTime>();
        //private Dictionary<ParameterPairs, double> result = new Dictionary<ParameterPairs, double>();
        //每种参数组合，在所有交易日的盈利结果
        public Dictionary<FiveParameterPairs, SortedDictionary<DateTime, double>> newResult = new Dictionary<FiveParameterPairs, SortedDictionary<DateTime, double>>();
        //按交易日选取出对应的参数组合
        public Dictionary<DateTime, FiveParameterPairs> parameters = new Dictionary<DateTime, FiveParameterPairs>();

        private Dictionary<DateTime, int> tradeIndexStart = new Dictionary<DateTime, int>();
        private Dictionary<DateTime, int> timeList = new Dictionary<DateTime, int>();

        public ERParametersChoice(int startDate, int endDate, string underlying, int choicePeriod, int serviceLife)
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
            //调用computeParameters，计算出newResult
            computeParameters();
            //调用chooseParameters，使用前choicePeriod交易日的数据，计算出最优参数对，作为接下来serviceLife时间段的参数配置
            parameters = chooseParameters(newResult, this.startDate, this.endDate, choicePeriod, serviceLife);
        }

        /// <summary>
        /// 计算参数
        /// </summary>
        private void computeParameters()
        {
            List<FuturesMinute> data = new List<FuturesMinute>();
            //获取首个回测日之前的三日（日）数据
            data = getData(DateUtils.PreviousTradeDay(tradeDays[0], 3), underlying);
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
            ParaPairs pairs = new ParaPairs();

            #region debug数据，请勿删除
            //int[] frequencySet = new int[] { 5 };
            //int[] numbersSet = new int[] { 10 };
            //double[] lossPercentSet = new double[] { 0.01 };
            //double[] ERRatioSet = new double[] { 0.75 };

            //int[] frequencySet = new int[] { 5, 10, 15, 20, 30 };
            //int[] numbersSet = new int[] { 3, 4, 5, 6, 8, 10 };
            //double[] lossPercentSet = new double[] { 0.000625, 0.00125, 0.0025, 0.005, 0.01 };
            //double[] ERRatioSet = new double[] { 0.5, 0.6, 0.7, 0.75, 0.8, 0.9 };
            #endregion

            int[] frequencySet = new int[] { 3, 5, 7, 10 };
            int[] numbersSet = new int[] { 3, 4, 5, 6, 8, 10, 15 };
            double[] lossPercentSet = new double[] { 0.000625, 0.00125, 0.0025, 0.005, 0.01, 0.015 };
            double[] longERSet = new double[] { 0.5, 0.6, 0.7, 0.8, 0.9 };
            double[] shortERSet = new double[] { -0.5, -0.6, -0.7, -0.8, -0.9 };

            //记录frequency的边际分布
            List<double> frequencyDistrbution = new List<double>();

            foreach (var fre in frequencySet)
            {
                frequency = fre; //给定K线周期
                pairs.frequency = frequency;
                //data是（1分钟）K线数据，dataModified是（frequency周期）K线数据
                var dataModified = FreqTransferUtils.minuteToNMinutes(data, frequency);
                foreach (var num in numbersSet)
                {
                    numbers = num; //给定前推的（frequency周期）K线数量
                    pairs.numbers = numbers;
                    foreach (var loss in lossPercentSet)
                    {
                        lossPercent = loss; //给定追踪止损的参数
                        pairs.lossPercent = lossPercent;
                        foreach (var er in longERSet)
                        {
                            longER = er; //给定ER比例的参数
                            pairs.longER = longER;
                            foreach (var shortEr in shortERSet)
                            {
                                shortER = shortEr;
                                pairs.shortER = shortER;

                                #region ...
                                double profitInDay = 0;
                                double positionVolume = 0;
                                double openPrice = 0;
                                double maxIncomeIndividual = 0;
                                Console.WriteLine("开始回测参数--> K线：{0}, 回望时间：{1}，追踪止损：{2}，longER值：{3}, shortER值：{4}", 
                                    pairs.frequency, pairs.numbers,  pairs.lossPercent, pairs.longER, pairs.shortER);

                                //[新版]记录该组参数对应的, 所有交易日的收益
                                FiveParameterPairs newPairs0 = new FiveParameterPairs
                                {
                                    longER = pairs.longER,
                                    shortER = pairs.shortER,
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
                                    if (i == dataModified.Count() - 1 || (i + 1 < dataModified.Count() && dataModified[i + 1].tradeday > now.tradeday))
                                    {
                                        //强制平仓
                                        if (positionVolume != 0)
                                        {
                                            //减去2倍的滑点，是因为买入和卖出均有手续费
                                            profitInDay += positionVolume * (now.open - openPrice) - 2 * slipPoint;
                                            //   Console.WriteLine("时间：{0}，价格：{1}, volume：0", dataModified[i].time, now.open);
                                        }

                                        //记录该组参数当日收益（一个交易日记录一次数据）
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
                                            //导入（前numbersK线）的收盘价
                                            prices[k - (i - numbers)] = dataModified[k].close;
                                        }
                                        //计算出ER值
                                        double ER = computeER(prices);
                                        if (positionVolume == 0) //持空仓
                                        {
                                            if (ER > longER && now.open > now.low) //开多仓,且能够开仓
                                            {
                                                openPrice = now.open;
                                                positionVolume = 1;
                                                // Console.WriteLine("时间：{0}，价格：{1}, volume：1", dataModified[i].time, now.open);
                                            }
                                            if (ER < shortER && now.open < now.high) //开空仓
                                            {
                                                openPrice = now.open;
                                                positionVolume = -1;
                                                // Console.WriteLine("时间：{0}，价格：{1}, volume：-1", dataModified[i].time, now.open);
                                            }
                                        }
                                        else if (positionVolume == 1) //持多仓
                                        {
                                            if ((now.open - openPrice) > maxIncomeIndividual)
                                            {
                                                maxIncomeIndividual = now.open - openPrice;
                                            }
                                            //追踪止损，强制平仓
                                            else if (((now.open - openPrice) - maxIncomeIndividual) < -lossPercent * now.open)
                                            {
                                                profitInDay += now.open - openPrice - 2 * slipPoint;

                                                //重置数据
                                                positionVolume = 0;
                                                maxIncomeIndividual = 0;
                                                // Console.WriteLine("时间：{0}，价格：{1}, volume：0", dataModified[i].time, now.open);
                                            }
                                        }
                                        else if (positionVolume == -1) //持空仓
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
                                #endregion
                            }

                        }
                    }
                }
                //第一层循环底部....

            }
        }

        /// <summary>
        /// 选择最优参数对。用前choicePeriod个交易日的数据计算出最优参数对，作为之后serviceLife个交易日的交易参数
        /// </summary>
        /// <param name="result"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="choicePeriod"></param>
        /// <param name="serviceLife"></param>
        /// <returns></returns>
        private Dictionary<DateTime, FiveParameterPairs> chooseParameters(Dictionary<FiveParameterPairs, SortedDictionary<DateTime, double>> result, DateTime startDate, DateTime endDate, int choicePeriod, int serviceLife)
        {

            DateTime start = startDate;
            DateTime end = DateUtils.NextTradeDay(start, choicePeriod - 1);
            DateTime paraStart = DateUtils.NextTradeDay(start, choicePeriod);
            DateTime paraEnd = DateUtils.NextTradeDay(start, choicePeriod + serviceLife - 1);
            //如果choicePeriod的数值，超过了回测周期的长度。做一些特殊处理。
            if (end > endDate)
            {
                end = endDate;
                paraStart = start;
                paraEnd = end;
            }

            ///最高分。因为参数对计算得分可能会出现负数，所以这里初始默认值不能为0。
            ///否则会出现循环所有参数对结束之后，最优参数对bestPara依然没有复制的情况。
            double marks = -1000000;

            //用来记录（在choicePeriod时间段中）得分最高的参数对
            FiveParameterPairs bestPara = new FiveParameterPairs();
            //记录每个交易日的最佳参数对（Dictionary类型）
            Dictionary<DateTime, FiveParameterPairs> paras = new Dictionary<DateTime, FiveParameterPairs>();
            //记录每个交易日使用的参数对（List类型）,用于输出到CSV文件
            List<ParaPairsWithScore> parasWithScore = new List<ParaPairsWithScore>();

            while (end <= endDate)
            {
                //循环所有的参数组合，选出（在choicePeriod时间段中）得分最高的参数对
                foreach (var item in result)
                {

                    double mark0 = getMarks(result, item.Key, start, end);
                    if (mark0 > marks)
                    {
                        marks = mark0;
                        bestPara = item.Key;
                    }
                }

                if (paras.Count() == 0)
                {
                    List<DateTime> dates0 = DateUtils.GetTradeDays(start, end);
                    foreach (var item in dates0)
                    {
                        paras.Add(item, bestPara);
                        parasWithScore.Add(new ParaPairsWithScore
                        {
                            tradeday = item,
                            frequency = bestPara.frequency,
                            numbers = bestPara.numbers,
                            lossPercent = bestPara.lossPercent,
                            longER = bestPara.longER,
                            shortER = bestPara.shortER,
                            Score = marks
                        });
                    }
                }
                //将选出的（choicePeriod时间段中）最优参数对，作为最优解保存到（ choicePeriod + serviceLife时间段）
                List<DateTime> dates = DateUtils.GetTradeDays(paraStart, paraEnd);
                foreach (var item in dates)
                {
                    paras.Add(item, bestPara);
                    parasWithScore.Add(new ParaPairsWithScore
                    {
                        tradeday = item,
                        frequency = bestPara.frequency,
                        numbers = bestPara.numbers,
                        lossPercent = bestPara.lossPercent,
                        longER = bestPara.longER,
                        shortER = bestPara.shortER,
                        Score = marks
                    });
                }

                //计算日期往后顺延serviceLife时间段，重置相关数据
                start = DateUtils.NextTradeDay(start, serviceLife);
                end = DateUtils.NextTradeDay(end, serviceLife);
                paraStart = DateUtils.NextTradeDay(paraStart, serviceLife);
                paraEnd = DateUtils.NextTradeDay(paraEnd, serviceLife);
                marks = -1000000;
                bestPara = new FiveParameterPairs();
                if (end >= endDate)
                {
                    break;
                }
            }

            //每个交易日使用的策略，写入CSV文件
            //List<ParameterPairs> tradeDaysWithBestParas = convertType(paras);
            string recordName = underlying.Replace(".", "_") + "_" + startDate.ToShortDateString().Replace("/", "_") + "_to_" + endDate.ToShortDateString().Replace("/", "_");
            RecordUtil.recordToCsv(data: parasWithScore, type: "tradeDaysWithBestParas", tag: GetType().FullName, parameters: recordName, performance: "");

            return paras;
        }

        /// <summary>
        /// 获得所有参数对的评分
        /// </summary>
        /// <param name="result"></param>
        /// <param name="pair"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public double getMarks(Dictionary<FiveParameterPairs, SortedDictionary<DateTime, double>> result, FiveParameterPairs pair, DateTime startDate, DateTime endDate)
        {
            List<double> netvalue = new List<double>();//净值
            List<double> returnRatio = new List<double>();//收益率
            netvalue.Add(1);//【问题】netvalue这里为什么要先add一个1，而returnRatio没有？
            double total = initialCapital;
            foreach (var item in result)
            {
                if (item.Key.longER == pair.longER && item.Key.shortER==pair.shortER && item.Key.frequency == pair.frequency &&
                    item.Key.lossPercent == pair.lossPercent && item.Key.numbers == pair.numbers)
                {
                    foreach (var num in item.Value)
                    {
                        if (num.Key >= startDate && num.Key <= endDate)
                        {
                            total += num.Value;
                            netvalue.Add(total / initialCapital);
                            returnRatio.Add(num.Value / initialCapital);
                        }
                    }
                    break;
                }
            }
            //获取最大回撤率
            double MDD = PerformanceStatisicsUtils.computeMaxDrawDown(netvalue);

            double sum = 0;//和
            double squareSum = 0;//平方和
            for (int i = 0; i < returnRatio.Count(); i++)
            {
                sum += returnRatio[i];
                squareSum += Math.Pow(returnRatio[i], 2);
            }
            double average = sum / returnRatio.Count();
            double std = Math.Sqrt(squareSum / returnRatio.Count() - average * average);
            double sharpe = average * 252 / (std * Math.Sqrt(252));//夏普率   //average * 252=年化收益

            //处理MDD为0的情况
            //double calmar = (MDD == 0 ? 4 : average * 252 / MDD);//Calmar比率
            //return (0.5 * sharpe + 0.5 * calmar) / 8;

            //不处理MDD为0的情况
            //double calmar = (MDD == 0 ? 4 : average  / MDD);//Calmar比率
            //return (0.5 * sharpe + 0.5 * calmar)/8;

            //只用年化收益率来打分
            return average * 252;
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
