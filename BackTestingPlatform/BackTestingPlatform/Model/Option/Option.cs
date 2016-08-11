using BackTestingPlatform.Model.Common;
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
    public class OptionInfo
    {
        public string optionCode { get; set; }
        public string optionName { get; set; }
        public string executeType { get; set; }
        public double strike { get; set; }
        public string optionType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        //public Dictionary<DateTime, string> IHcodes;

    }



    public class OptionTickData : Sequential
    {

        public DateTime time { get; set; }
        public double lastPrice;
        public Common.Position[] ask, bid;
        public OptionGreek greek;
        public Tick underlyingStock;
        public FuturesTickData underlyingFutures;

    }

    public struct OptionGreek
    {
        public double sigma, delta, gamma, vega, theta;
    }

    public class OptionMinuteData : KLine
    {
    
    }

    public class OptionMinuteDataWithUnderlying : OptionMinuteData
    {
        public double underlyingPrice { get; set; }
        public string optionCode { get; set; }
        public string optionName { get; set; }
        public string executeType { get; set; }
        public double strike { get; set; }
        public string optionType { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }

}
