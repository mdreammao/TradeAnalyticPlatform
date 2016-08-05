using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{
    public class Tick
    {
        public string code;
        public DateTime time;
        public double lastPrice;
        public PositionData[] ask, bid;
        public double preClose;

    }
    public class TickFromMssql
    {
        public string code;
        public int date,time;
        public double lastPrice;
        public PositionData[] ask, bid;
        public double highPrice, lowPrice, volume, turnoverVolume;
        public double preClose,preSettle;
    }
}
