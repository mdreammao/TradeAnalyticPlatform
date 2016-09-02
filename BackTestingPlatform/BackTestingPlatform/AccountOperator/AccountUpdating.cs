using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Utilities.TimeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction.TransactionWithSlip
{
    public class AccountUpdating
    {
        //起始资金
        public static double intialCapital = 10000000;

        /// <summary>
        /// 账户信息更新，包括计算保证金，持仓数据汇总
        /// </summary>
        /// <param name="myAccount"></param>当前账户
        /// <param name="nowPosition"></param>当前持仓，用于计算保证金及持仓价值
        /// <param name="now"></param>当前时间
        /// <param name="data"></param>当天行情数据
        public static void computeAccountUpdating(ref BasicAccount myAccount, Dictionary<string, PositionsWithDetail> nowPosition, DateTime now, ref Dictionary<string, List<KLine>> data)
        {
            
            //计算保证金
            double totalMargin = CalculateMargin.calculateMargin(nowPosition, now, ref data);
            //计算剩余可用资金
            //持仓的资金流加总
            double totalCashFlow = 0;    
            //计算持仓总价值
            //持仓价值（实时）
            //当前时间对应data中timeList 的序号
            int index = TimeListUtility.MinuteToIndex(now);
            double totalPositionValue = 0;
            foreach (var position0 in nowPosition.Values)
            {
                //累加持仓现金流
                //position0.totalCost记录的是带头寸方向的总成本，因此正数为支出cash建立多头，负数为收取cash，支出保证金建立空头
                totalCashFlow += -position0.totalCost;
                //累加持仓价值（实时）
                //当前持仓成本价
                double nowPositionAveragePrice = position0.volume > 0 ? position0.LongPosition.averagePrice : position0.ShortPosition.averagePrice;
                totalPositionValue += (data[position0.code][index].close - nowPositionAveragePrice)* position0.volume + Math.Abs(nowPositionAveragePrice * position0.volume);

            }
            //剩余可用资金 = 初始资本 + 持仓的资金流加总（开仓、手续费支出为负，平仓为正）- 保证金
            double freeCash = intialCapital + totalCashFlow - totalMargin;
            myAccount.freeCash = freeCash;
            myAccount.margin = totalMargin;
            myAccount.positionValue = totalPositionValue;
            //总资产 = 持仓价值 + 保证金 + 剩余可用资金
            myAccount.totalAssets = totalPositionValue + totalMargin + freeCash;
            //当前时间
            myAccount.time = now;

        }

    }
}
