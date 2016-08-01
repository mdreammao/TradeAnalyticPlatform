using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model.Futures;

namespace BackTestingPlatform.Model.Option
{
    public class OptionTickData
    {
        public string optionCode;
        public DateTime time;
        public double lastPrice;
        public PositionData[] ask, bid;
        public OptionGreek greek;
        public TickData underlyingStock;
        public FuturesTickData underlyingFutures;
        public OptionInfo baseInfo;
    }

    public class OptionGreek
    {
        public double sigma, delta, gamma, vega, theta;
    }

    
}
