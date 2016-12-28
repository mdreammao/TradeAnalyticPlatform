using BackTestingPlatform.Strategies.Futures.MaoHeng.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng
{
    public class EfficiencyRatioWithParasStability
    {
        //回测参数设置
        private double initialCapital = 3000;
        private DateTime startDate, endDate;
        private Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> resultOfPeriod = new Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>>();

        public EfficiencyRatioWithParasStability(int startDate, int endDate, Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> result)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            this.resultOfPeriod = result;

            //对resultOfPeriod进行处理，选择出在startDate到endDate时间段的数据
            foreach (var item in resultOfPeriod)//resultOfPeriod类型 & item类型：Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>>
            {
                foreach(var item0 in item.Value.Keys)// item.Value.Keys类型 & item0类型：DateTime
                {
                    if (item0 < this.startDate || item0 > this.endDate)
                        item.Value.Remove(item0);
                }
            }

            //TODO:计算每个参数的边际分布...
            computeParasDistribution(resultOfPeriod);
        }

        /// <summary>
        /// 计算每个参数的边际分布
        /// </summary>
        /// <param name="resultOfPeriod"></param>
        public void computeParasDistribution(Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> resultOfPeriod)
        {
            //foreach(var item in resultOfPeriod)
            //{
            //    double frequencyBenchmark=
            //    item.Key.frequency
            //}
        }

        /// <summary>
        /// 获得每个参数对的评分
        /// </summary>
        /// <param name="result"></param>
        /// <param name="pair"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public double getMarks(Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> result, FourParameterPairs pair, DateTime startDate, DateTime endDate)
        {
            List<double> netvalue = new List<double>();//净值
            List<double> returnRatio = new List<double>();//收益率
            netvalue.Add(1);//【问题】netvalue这里为什么要先add一个1，而returnRatio没有？
            double total = initialCapital;
            foreach (var item in result)
            {
                if (item.Key.ERRatio == pair.ERRatio && item.Key.frequency == pair.frequency && item.Key.lossPercent == pair.lossPercent && item.Key.numbers == pair.numbers)
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
            double MDD = Math.Round(PerformanceStatisicsUtils.computeMaxDrawDown(netvalue), 4);

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
            double calmar = average * 252 / MDD;//Calmar比率
            return 0.5 * sharpe + 0.5 * calmar;
        }



    }
}
