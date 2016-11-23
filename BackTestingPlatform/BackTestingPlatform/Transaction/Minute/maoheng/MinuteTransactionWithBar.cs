using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction.Minute.maoheng
{
    /// <summary>
    /// 根据分钟K线图来判断成交。具体的，如果开多仓，开仓价格必须大于等于K线中的最低价格，如果开空仓，开仓价格必须小于等于K线中的最高价格。
    /// </summary>
    public class MinuteTransactionWithBar
    {
        public static DateTime ComputePosition(Dictionary<string, List<KLine>> data, ref SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, ref BasicAccount myAccount, DateTime now)
        {
            return now.AddMinutes(1);
        }
    }
}
