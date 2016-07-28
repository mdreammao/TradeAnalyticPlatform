using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies
{
    class TransactedJudge
    {

        //存放成交回报用于返回，[1]成交时间、[2]成交价、[3]成交量（考虑到追单到极限时间依然部分成交的情况）
        public double[] TransactionReturn;
        
        /// <summary>
        /// 当出现下单信号时，判断是否成交，返回成交价格、成交额、成交时间
        /// </summary>
        /// <param name="MAParam">股票代码</param>
        /// 
        /*
        public double[] Jugde()
        { 
        
        
        }
        
        */

    }
}
