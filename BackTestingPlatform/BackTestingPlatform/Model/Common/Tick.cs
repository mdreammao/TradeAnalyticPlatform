using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{

    public struct Tick : Sequential
    {
        public string code { get; set; }
        public DateTime time { get; set; }
        public double lastPrice { get; set; }
        public Position[] ask { get; set; }
        public Position[] bid { get; set; }
        public double preClose { get; set; }

    }

    public class TickFromMssql 
    {
        public string code;
        public int date, time;
        public double lastPrice;
        public Position[] ask, bid;
        public double highPrice, lowPrice, volume, turnoverVolume;
        public double preClose, preSettle;
    }
}
