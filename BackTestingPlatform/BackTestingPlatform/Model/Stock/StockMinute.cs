using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    public class StockMinute : KLine
    {

    }
    public class StockMinuteWithInfo : StockMinute
    {
        public StockInfo basicInfo { get; set; }
    }
}
