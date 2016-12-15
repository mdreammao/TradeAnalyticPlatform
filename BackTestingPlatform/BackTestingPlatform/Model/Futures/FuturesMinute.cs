using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Futures
{
    public class FuturesMinute : KLine
    {
        public int indexInDay { get; set; } //日内下标
        public DateTime tradeday { get; set; } //归属的交易日
    }

    public class FuturesMinuteWithInfo : FuturesMinute
    {
        public FuturesInfo basicInfo { get; set; }
    }
}
