using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Futures
{
    public class FuturesTickData
    {
        public string futuresCode;
        public DateTime time;
        public double lastPrice;
        public PositionData[] ask, bid;
        public FuturesInfo baseInfo;
    }
}
