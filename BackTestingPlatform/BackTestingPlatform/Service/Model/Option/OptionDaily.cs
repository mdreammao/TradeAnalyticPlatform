using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{

    public class OptionDailyWithInfo : OptionDaily
    {
        public OptionInfo basicInfo { get; set; }

    }


    public class OptionDaily : KLine
    {
        public double settle { get; set; } //结算价
        public double preSettle { get; set; } //前结算价
    }

}
