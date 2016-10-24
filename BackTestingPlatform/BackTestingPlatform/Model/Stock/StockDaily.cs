using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BackTestingPlatform.Model.Stock
{

    public class StockDailyWithInfo : KLine
    {
        public StockInfo basicInfo { get; set; }
    }
  
    public class StockDailyWithFactor : KLine
    {
        public StockFactor basicFactor { get; set; }
    }

    public class StockDailyWithFactor : KLine
    {
        public double settle { get; set; }
        public double preSettle { get; set; }
        public double preClose { get; set; }
        public double adjustFactor { get; set; }
    }
}
