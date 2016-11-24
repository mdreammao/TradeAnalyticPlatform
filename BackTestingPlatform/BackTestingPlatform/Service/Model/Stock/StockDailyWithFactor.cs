using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    public class StockDailyWithFactor : StockDaily
    {
        public double marketValue { get; set; }
        public double floatMarketValue { get; set; }
        public double PE { get; set; }
        public double PB { get; set; }
        public double PS { get; set; }
        public string industry { get; set; }
    }
}
