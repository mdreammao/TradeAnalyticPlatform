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
        /// <param name="nowTransactionCost"></param>当前交易费用
        /// <param name="now"></param>当前时间
        public static void computeAccountUpdating(ref BasicAccount myAccount, PositionsWithDetail nowPosition, double nowTransactionCost, DateTime now)
        {
            
            
             
        }

    }
}
