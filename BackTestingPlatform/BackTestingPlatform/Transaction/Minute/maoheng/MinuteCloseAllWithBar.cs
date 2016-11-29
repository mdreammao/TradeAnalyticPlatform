using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Service.Model.Positions;
using BackTestingPlatform.Utilities.TimeList;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction.Minute.maoheng
{
    public class MinuteCloseAllWithBar
    {
        public static Dictionary<string, ExecutionReport> CloseAllPosition(Dictionary<string, List<KLine>> data, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, DateTime now, double slipPoint = 0.00)
        {
            //初始化记录成交回报的变量
            Dictionary<string, ExecutionReport> tradingFeedback = new Dictionary<string, ExecutionReport>();
            //初始化平仓信号
            Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
            //查询当前持仓情况
            Dictionary<string, PositionsWithDetail> positionShot = new Dictionary<string, PositionsWithDetail>();
            Dictionary<string, PositionsWithDetail> positionLast = (positions.Count == 0 ? null : positions[positions.Keys.Last()]);
            //检查最新持仓，若无持仓，直接返回
            if (positionLast == null)
            {
                return tradingFeedback;
            }
            else
                positionShot = new Dictionary<string, PositionsWithDetail>(positionLast);
            //生成清仓信号
            foreach (var position0 in positionShot.Values)
            {
                //若当前品种持仓量为0（仅用于记录历史持仓）
                if (position0.volume == 0)
                    continue;
                //对所有的持仓，生成现价等量反向的交易信号
                int index = TimeListUtility.MinuteToIndex(now);
                MinuteSignal nowSignal = new MinuteSignal()
                {
                    code = position0.code,
                    volume = -position0.volume,
                    time = now,
                    tradingVarieties = position0.tradingVarieties,
                    price = data[position0.code][index].open,
                    minuteIndex = index
                };
                signal.Add(nowSignal.code, nowSignal);
            }
            return MinuteTransactionWithBar.ComputePosition(signal, data, ref positions, ref myAccount, slipPoint: slipPoint, now: now);
        }
    }
}
