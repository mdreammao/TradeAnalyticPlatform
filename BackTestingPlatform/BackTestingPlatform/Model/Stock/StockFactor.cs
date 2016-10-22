using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    public class StockFactor
    {
        public string code { get; set; }
        public double marketValue { get; set; }
        public double currentMarketValue { get; set; }
        public double PE { get; set; }
        public double PB { get; set; }
        public double PS { get; set; }
        public double industry { get; set; }

    }
}
