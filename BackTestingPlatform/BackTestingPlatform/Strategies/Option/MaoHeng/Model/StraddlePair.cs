using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.MaoHeng.Model
{
    public class StraddlePairCode
    {
        public string callCodeFront, callCodeNext, putCodeFront, putCodeNext;
    }
    public class StraddlePair 
    {
        public string callCodeFront, callCodeNext,putCodeFront,putCodeNext,IHCode;
        public double callPositionFront,callPositionNext, putPositionFront, putPositionNext,IHPosition, etfPosition;
        public DateTime endDate, straddleOpenDate,endDateNext;
        public double etfPrice_open, straddlePairPrice_open;
        public double strike;
    }
}
