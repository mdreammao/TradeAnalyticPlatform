using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{

    public class OptionMinute : KLine
    {
        
    }

    public class OptionMinuteWithInfo : OptionMinute
    { 
       public OptionInfo basicInfo { get; set; }
    }

}
