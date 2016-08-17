using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{

    public class OptionMinuteKLine : KLine
    {
    
    }

    public class OptionMinuteKLineWithUnderlying : OptionMinuteKLine
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
