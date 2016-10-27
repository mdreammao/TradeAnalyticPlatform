using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        //    public double anualReturn { get; set; }
        //    public double cumReturn { get; set; }
        //    public double anualSharpe { get; set; }
        //    public double winningRate { get; set; }
        //    public double PnLRatio { get; set; }
        //    public double annualSharpe { get; set; }
        //    public double maxDrawDown { get; set; }
        //    public double maxProfitRate { get; set; }
        //    public double returnMDDRatio { get; set; }
        //    public double informationRatio { get; set; }
        //    public double rSquare { get; set; }
        //    public double averageHoldingPeriod { get; set; }
        public static PerformanceStatisics compute(List<BasicAccount> accountHistory, SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions)
        {
            PerformanceStatisics performanceStats = new PerformanceStatisics();
            //初始资产
            double intialAssets = accountHistory[0].totalAssets;
            //净值
            double[] netWorth = accountHistory.Select(a => a.totalAssets / intialAssets).ToArray();
            //account长度，account记录周期数
            int lengthOfAccount = accountHistory.Count;
            //交易次数
            int numOfTrades = 0;
            //成功交易次数
            int numOfSuccess = 0;
            //失败交易次数
            int numOfFailure = 0;
            //交易统计
            foreach (var date in positions.Keys)
            {
                foreach (var variety in positions[date].Keys)
                {
                    //交易笔数累计（一组相邻的反向交易为一笔交易）
                    numOfTrades = positions[date][variety].record.Count / 2;
                    //成功交易笔数累计
                    

                }

            }

            // netProfit
            performanceStats.netProfit = accountHistory[lengthOfAccount - 1].totalAssets - accountHistory[0].totalAssets; 

            //

            
            



            return performanceStats;
        }


    }
}
