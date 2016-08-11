using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{
    public struct Kline
    {
        public DateTime time { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public double amount { get; set; }
        public double openInterest { get; set; }//持仓量
    }
}
