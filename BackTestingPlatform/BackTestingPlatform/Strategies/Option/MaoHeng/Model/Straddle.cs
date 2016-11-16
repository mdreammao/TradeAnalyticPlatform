using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.MaoHeng.Model
{
    public class Straddle
    {
        public string callCode, putCode,IHCode;
        public double callPosition, putPosition,IHPosition;
        public DateTime endDate, straddleOpenDate;
        public double etfPrice_open, straddlePrice_open;
        public double strike;
    }
}
