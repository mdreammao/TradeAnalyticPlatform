using BackTestingPlatform.Strategies.Futures.MaoHeng.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Charts;
using IronPython.Modules;
using System.Windows.Forms;
using Microsoft.Scripting.Utils;

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
            //foreach (var item in resultOfPeriod)//resultOfPeriod类型 & item类型：Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>>
            //{
            //    foreach(var item0 in item.Value.Keys)// item.Value.Keys类型 & item0类型：DateTime
            //    {
            //        if (item0 < this.startDate || item0 > this.endDate)
            //            item.Value.Remove(item0);
            //    }
            //}

            //TODO:计算每个参数的边际分布...
            computeParasDistribution(resultOfPeriod);
        }

        /// <summary>
        /// 计算每个参数的边际分布
        /// </summary>
        /// <param name="resultOfPeriod"></param>
        public void computeParasDistribution(Dictionary<FourParameterPairs, SortedDictionary<DateTime, double>> resultOfPeriod)
        {      
            Dictionary<double, double> frequencyDictionary = new Dictionary<double, double>();//记录<frequency参数>以及<其参数所有参数对的分数总和>的字典
            Dictionary<double, double> numbersDictionary = new Dictionary<double, double>();//记录<numbers参数>以及<其参数所有参数对的分数总和>的字典
            Dictionary<double, double> lossPercentDictionary = new Dictionary<double, double>();//记录<lossPercent参数>以及<其参数所有参数对的分数总和>的字典
            Dictionary<double, double> ERRatioDictionary = new Dictionary<double, double>();//记录<ERRatio参数>以及<其参数所有参数对的分数总和>的字典

            //所有包含frequency参数对的总个数。单个frequency参数的参数对个数=frequencyCount/frequencyDictionary.Count;
            int frequencyCount = 0;
            int numbersCount = 0;
            int lossPercentCount = 0;
            int ERRatioCount = 0;

            //遍历数据
            foreach (var item in resultOfPeriod)
            {
                //获取该参数对的得分
                double singlePairMarks = getKeyValuePairMarks(item);

                //如果frequencyDictionary中的key未包含该参数对中的frequency,则新增;否则value值叠加。
                if ( ! frequencyDictionary.Keys.Contains(item.Key.frequency))
                {
                    frequencyDictionary.Add(item.Key.frequency, singlePairMarks);
                    frequencyCount++;
                }
                else
                {
                    frequencyDictionary[item.Key.frequency] += singlePairMarks;
                    frequencyCount++;
                }

                //numbersDictionary...
                if ( ! numbersDictionary.Keys.Contains(item.Key.numbers))
                {
                    numbersDictionary.Add(item.Key.numbers,singlePairMarks);
                    numbersCount++;
                }
                else
                {
                    numbersDictionary[item.Key.numbers] += singlePairMarks;
                    numbersCount++;
                }

                //lossPercentDictionary...
                if ( ! lossPercentDictionary.Keys.Contains(item.Key.lossPercent))
                {
                    lossPercentDictionary.Add(item.Key.lossPercent,singlePairMarks);
                    lossPercentCount++;
                }
                else
                {
                    lossPercentDictionary[item.Key.lossPercent] += singlePairMarks;
                    lossPercentCount++;
                }

                //ERRatioDictionary...
                if ( ! ERRatioDictionary.Keys.Contains(item.Key.ERRatio))
                {
                    ERRatioDictionary.Add(item.Key.ERRatio,singlePairMarks);
                    ERRatioCount++;
                }
                else
                {
                    ERRatioDictionary[item.Key.ERRatio] += singlePairMarks;
                    ERRatioCount++;
                }
            } //结束遍历数据...

            //绘制4幅图...

            //frequency图像
            Dictionary<string, double[]> frequencyLine = new Dictionary<string, double[]>();
            double[] frequencyMarks = frequencyDictionary.Values.Select(x => x/(frequencyCount/frequencyDictionary.Count)).ToArray();
            frequencyLine.Add("frequency", frequencyMarks);
            string[] frequencyStr = frequencyDictionary.Keys.Select(x => x.ToString()).ToArray();
            Application.Run(new PLChart(frequencyLine, frequencyStr,startDate.ToShortDateString()+" - "+endDate.ToShortDateString()+ " " + "frequency参数稳定性曲线","参数值","得分均值"));

            //numbers图像
            Dictionary<string, double[]> numbersLine = new Dictionary<string, double[]>();
            double[] numbersMarks = numbersDictionary.Values.Select(x => x/(numbersCount/numbersDictionary.Count)).ToArray();
            numbersLine.Add("numbers",numbersMarks);
            string[] numbersStr = numbersDictionary.Keys.Select(x => x.ToString()).ToArray();
            Application.Run(new PLChart(numbersLine,numbersStr, startDate.ToShortDateString() + " - " + endDate.ToShortDateString() + " " + "numbers参数稳定性曲线", "参数值", "得分均值"));

            //lossPercent图像
            Dictionary<string, double[]> lossPercentLine = new Dictionary<string, double[]>();
            double[] lossPercentMarks = lossPercentDictionary.Values.Select(x => x/(lossPercentCount/lossPercentDictionary.Count)).ToArray();
            lossPercentLine.Add("lossPercent",lossPercentMarks);
            string[] lossPercentStr = lossPercentDictionary.Keys.Select(x => x.ToString()).ToArray();
            Application.Run(new PLChart(lossPercentLine,lossPercentStr, startDate.ToShortDateString() + " - " + endDate.ToShortDateString() + " " + "lossPercent参数稳定性曲线", "参数值", "得分均值"));

            //ERRatio图像
            Dictionary<string, double[]> ERRatioLine = new Dictionary<string, double[]>();
            double[] ERRatioMarks = ERRatioDictionary.Values.Select(x => x/(ERRatioCount/ERRatioDictionary.Count)).ToArray();
            ERRatioLine.Add("ERRatio",ERRatioMarks);
            string[] ERRatioStr = ERRatioDictionary.Keys.Select(x => x.ToString()).ToArray();
            Application.Run(new PLChart(ERRatioLine,ERRatioStr, startDate.ToShortDateString() + " - " + endDate.ToShortDateString() + " "+"ERRatio参数稳定性曲线", "参数值", "得分均值"));
        }

        /// <summary>
        /// 获得单个参数对的评分
        /// </summary>
        /// <param name="parameterPair"></param>
        /// <returns></returns>
        public double getKeyValuePairMarks(KeyValuePair<FourParameterPairs, SortedDictionary<DateTime, double>> parameterPair)
        {
            List<double> netvalue = new List<double>();//净值
            List<double> returnRatio = new List<double>();//收益率
            netvalue.Add(1);//【问题】netvalue这里为什么要先add一个1，而returnRatio没有？
            double total = initialCapital;

            foreach (var num in parameterPair.Value)
            {
                if (num.Key >= startDate && num.Key <= endDate)
                {
                    total += num.Value;
                    netvalue.Add(total / initialCapital);
                    returnRatio.Add(num.Value / initialCapital);
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
            //double calmar = (MDD == 0 ? 4 : average / MDD);//Calmar比率
            //return (0.5 * sharpe + 0.5 * calmar) / 8;

            //只用年化收益率来打分(*100,变为百分制)
            return average*252*100;

        }


    }
}
