using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.TradeOptionByMA.Model
{
    public class StranglePair
    {
        public string callCode, putCode;
        public double callPosition, putPosition;
        public DateTime endDate;
        public double etfPrice;
        public double callStrike, putStrike;
    }
}
