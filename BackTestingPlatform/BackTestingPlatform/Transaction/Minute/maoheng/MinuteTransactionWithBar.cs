using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction.Minute.maoheng
{
    /// <summary>
    /// 根据分钟K线图来判断成交。具体的，如果开多仓，开仓价格必须大于等于K线中的最低价格，如果开空仓，开仓价格必须小于等于K线中的最高价格。
    /// 按品种分别讨论，分成股票，期权和期货三类来讨论。
    /// </summary>
    public class MinuteTransactionWithBar
    {
        public static DateTime ComputePosition(Dictionary<string, MinuteSignal> signal, Dictionary<string, List<KLine>> data, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, DateTime now, double slipPoint = 0.003)
        {
            //如果signal无信号，无法成交，直接返回下一分钟时刻。
            if (signal==null || signal.Count==0)
            {
                return now.AddMinutes(1);
            }
            //新建头寸变量，作为接受新仓位的容器
            Dictionary<string, PositionsWithDetail> positionShot = new Dictionary<string, PositionsWithDetail>();
            //获取前一步的头寸信息，如果没有寸头就设为null
            Dictionary<string, PositionsWithDetail> positionLast = (positions.Count == 0 ? null : positions[positions.Keys.Last()]);
            //若上一时刻持仓不为空，上刻持仓先赋给此刻持仓，再根据信号调仓
            if (positionLast != null)
            {
                positionShot = new Dictionary<string, PositionsWithDetail>(positionLast);
            }
            //对信号进行遍历
            foreach (var signal0 in signal.Values)
            {
                //有效信号才进行操作
                if (signal0.volume!=0)
                {

                }

            }

            return now.AddMinutes(1);
        }
        
    }
}
