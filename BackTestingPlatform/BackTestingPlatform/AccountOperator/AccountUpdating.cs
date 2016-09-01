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
        /// <summary>
        /// 账户信息更新，包括计算保证金，持仓数据汇总
        /// </summary>
        /// <param name="myAccount"></param>当前账户
        /// <param name="nowPosition"></param>当前持仓，用于计算保证金及持仓价值
        /// <param name="nowCashFlow"></param>当前产生的现金流，若进行开仓操作，支出现价，现金流为负，若进行平仓操作则反之
        /// <param name="now"></param>当前时间
        /// <param name="data"></param>当天行情数据

        public static void computeAccountUpdating(ref BasicAccount myAccount, PositionsWithDetail nowPosition, double nowCashFlow, DateTime now, ref Dictionary<string, List<KLine>> data)
        {
            
            
            
             
        }

    }
}
