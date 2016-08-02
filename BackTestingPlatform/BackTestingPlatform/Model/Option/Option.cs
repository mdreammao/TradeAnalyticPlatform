using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{
    /// <summary>
    /// 期权 基础信息
    /// </summary>
    public class Option
    {
        public string code;     //期权代码
        public string name;     //期权名称
        public string executeType;
        public double strike;
        public string optionType;
        public DateTime startDate, endDate;

        public Dictionary<DateTime, string> IHcodes;
        public List<OptionTickData> ticks;
    }

    public class OptionTickData
    {
        
        public DateTime time;
        public double lastPrice;
        public PositionData[] ask, bid;
        public OptionGreek greek;
        public StockTickData underlyingStock;
        public FuturesTickData underlyingFutures;
        
    }

    public struct OptionGreek
    {
        public double sigma, delta, gamma, vega, theta;
    }

}
