using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model
{
    class TickPositionData
    {
        string stkcd{get;set;}
        DateTime time;
        double cp;
        
    }

    class MinuteData
    {
        string stockName { get; set; }
        DateTime time;
        double open, high, low, close, volume;
    }
}
