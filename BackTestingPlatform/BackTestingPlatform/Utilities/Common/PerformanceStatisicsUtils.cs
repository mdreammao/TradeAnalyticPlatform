using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.LinearRegression;

namespace BackTestingPlatform.Utilities.Common
{
    public class PerformanceStatisicsUtils
    {
        /// <summary>
        /// 计算策略的各项性能指标，目前胜率等和交易次数相关的指标只针对非仓位管理型的策略
        /// </summary>
        /// <param name="accountHistory"></param>
        /// <param name="positions"></param>
        /// <returns></returns>
        //    public double netProfit { get; set; }
        //    public double perNetProfit { get; set; }
        //    public double totalReturn { get; set; }
        //    public double anualReturn { get; set; }
        //    public double anualSharpe { get; set; }
        //    public double winningRate { get; set; }
        //    public double PnLRatio { get; set; }
        //    public double annualSharpe { get; set; }
        //    public double maxDrawDown { get; set; }
        //    public double maxProfitRate { get; set; }
        //    public double ProfitMDDRatio { get; set; }//收益回撤比
        //    public double informationRatio { get; set; }//信息比率
        //    public double alpha { get; set; }//Jensen' alpha
        //    public double beta { get; set; }//beta系数，CAPM模型
        //    public double rSquare { get; set; }//R平方，线性拟合优度
        //    public double averageHoldingRate { get; set; }//平均持仓比例
        //    public double averagePositionRate { get; set; }//平均仓位
        public static PerformanceStatisics compute(List<BasicAccount> accountHistory, SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, double[] benchmark)
        {
            PerformanceStatisics performanceStats = new PerformanceStatisics();
            //无风险收益率(年化)
            double riskFreeRate = 0.03;
            //account长度，account记录周期数
            int lengthOfAccount = accountHistory.Count;
            //初始资产
            double intialAssets = accountHistory[0].totalAssets;
            //净值
            double[] netWorth = accountHistory.Select(a => a.totalAssets / intialAssets).ToArray();
            //收益率与超额收益率，比净值数少一
            double[] returnArray = new double[netWorth.Length - 1];//收益率
            double[] returnArrayOfBenchmark = new double[netWorth.Length - 1];//基准收益率
            double[] benchmarkExcessReturn = new double[returnArray.Length];//基准收益率 - 无风险收益率
            double[] excessReturnToBenchmark = new double[returnArray.Length];//收益率 - 基准收益率
            double[] excessReturnToRf = new double[returnArray.Length];//收益率 - 无风险收益率
            double[] timeIndexList = new double[netWorth.Length];//时间标签tick
            for (int i = 0; i < returnArray.Length; i++)
            {
                returnArray[i] = (netWorth[i + 1] - netWorth[i]) / netWorth[i];
                returnArrayOfBenchmark[i] = (benchmark[i + 1] - benchmark[i]) / benchmark[i];
                excessReturnToRf[i] = returnArray[i] - riskFreeRate;
                benchmarkExcessReturn[i] = returnArrayOfBenchmark[i] - riskFreeRate;
                excessReturnToBenchmark[i] = returnArray[i] - returnArrayOfBenchmark[i];
                timeIndexList[i] = i;
            }
            timeIndexList[timeIndexList.Length - 1] = timeIndexList.Length - 1;
            //交易次数
            double numOfTrades = 0;
            //成功交易次数
            double numOfSuccess = 0;
            //失败交易次数
            double numOfFailure = 0;
            //累计盈利
            double cumProfit = 0;
            //累计亏损
            double cumLoss = 0;

            //交易统计
            foreach (var date in positions.Keys)
            {
                foreach (var variety in positions[date].Keys)
                {
                    //交易笔数累计（一组相邻的反向交易为一笔交易）
                    numOfTrades += positions[date][variety].record.Count / 2;
                    //成功交易笔数累计
                    //  List<TransactionRecord> lastestRecord = new List<TransactionRecord>(positions[date][variety].record[positions[date][variety].record.Count -1])
                    for (int rec = 1; rec < positions[date][variety].record.Count; rec += 2)
                    {
                        var nowRec = positions[date][variety].record[rec];
                        var lastRec = positions[date][variety].record[rec - 1];
                        //若当前为平多，则平多价格大于开多价格，成功数+1；
                        //若当前为平空，则平空价格小于于开空价格，成功数+1
                        if ((nowRec.volume < 0 && nowRec.price > lastRec.price) || (nowRec.volume > 0 && nowRec.price < lastRec.price))
                        {
                            //成功计数
                            numOfSuccess++;
                            //收益累加
                            cumProfit += nowRec.volume < 0 ? (nowRec.price - lastRec.price) * Math.Abs(nowRec.volume) : (-nowRec.price + lastRec.price) * Math.Abs(nowRec.volume);
                        }
                        else
                        {
                            //亏损累加
                            cumLoss += nowRec.volume < 0 ? (nowRec.price - lastRec.price) * Math.Abs(nowRec.volume) : (-nowRec.price + lastRec.price) * Math.Abs(nowRec.volume);
                        }
                    }
                }
            }
            numOfFailure = numOfTrades - numOfSuccess;

            // netProfit
            performanceStats.netProfit = accountHistory[lengthOfAccount - 1].totalAssets - intialAssets;

            //perNetProfit
            performanceStats.perNetProfit = performanceStats.netProfit / numOfTrades;

            //totalReturn
            performanceStats.totalReturn = performanceStats.netProfit / intialAssets;

            //anualReturn
            double daysOfBackTesting = accountHistory.Count;
            performanceStats.anualReturn = performanceStats.totalReturn / (daysOfBackTesting / 252);

            //anualSharpe
            performanceStats.anualSharpe = (returnArray.Average() - riskFreeRate) / Statistics.StandardDeviation(returnArray) * Math.Sqrt(252);

            //winningRate
            performanceStats.winningRate = numOfSuccess / numOfTrades;

            //PnLRatio
            performanceStats.PnLRatio = cumProfit / Math.Abs(cumLoss);

            //maxDrawDown
            performanceStats.maxDrawDown = computeMaxDrawDown(netWorth.ToList());

            //maxProfitRate
            performanceStats.maxProfitRatio = computeMaxProfitRate(netWorth.ToList());

            //profitMDDRatio
            performanceStats.profitMDDRatio = performanceStats.totalReturn / performanceStats.maxDrawDown;

            //informationRatio


            performanceStats.informationRatio = excessReturnToBenchmark.Average() / Statistics.StandardDeviation(excessReturnToBenchmark) * Math.Sqrt(252);

            //alpha
            var regstats = SimpleRegression.Fit(benchmarkExcessReturn, excessReturnToRf);
            performanceStats.alpha = regstats.Item1;

            //beta
            performanceStats.beta = regstats.Item2;

            //rSquare
            performanceStats.rSquare = Math.Pow(Correlation.Pearson(timeIndexList, netWorth),2);

            //averageHoldingRate 
            double barsOfHolding = 0;
            double[] positionRate = new double[accountHistory.Count];
            int sign = 0;
            foreach (var accout in accountHistory)
            {
                if (accout.positionValue != 0) barsOfHolding++;
                positionRate[sign] = accout.positionValue / accout.totalAssets;
                sign++;
            }

            performanceStats.averageHoldingRate = barsOfHolding / accountHistory.Count;

            //averagePositionRate
            performanceStats.averagePositionRate = positionRate.Average();

            return performanceStats;
        }



        /// <summary>
        /// 计算最大回撤率
        /// </summary>
        /// <param name="price"></param>传入double数组的价格序列
        /// <returns></returns>
        private static double computeMaxDrawDown(List<double> price)
        {
            double maxDrawDown = 0;
            double[] MDDArray = new double[price.Count];

            for (int i = 0; i < price.Count; i++)
            {
                double tempMax = price.GetRange(0, i + 1).Max();
                if (tempMax == price[i])
                    MDDArray[i] = 0;
                else
                    MDDArray[i] = Math.Abs((price[i] - tempMax) / tempMax);
            }
            maxDrawDown = MDDArray.Max();
            return maxDrawDown;
        }

        /// <summary>
        /// 计算最大净值升水率
        /// </summary>
        /// <param name="price"></param>传入double数组的价格序列
        /// <returns></returns>
        private static double computeMaxProfitRate(List<double> price)
        {
            double maxProfitRate = 0;
            double[] MPRArray = new double[price.Count];

            for (int i = 0; i < price.Count; i++)
            {
                double tempMin = price.GetRange(0, i + 1).Min();
                if (tempMin == price[i])
                    MPRArray[i] = 0;
                else
                    MPRArray[i] = Math.Abs((price[i] - tempMin) / tempMin);
            }
            maxProfitRate = MPRArray.Max();
            return maxProfitRate;
        }

    }


}
