using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.TradeOptionByMA.model
{
    public class Strangle
    {
        public string callCode, putCode;
        public double callPosition, putPosition;
        public DateTime endDate;
        public double etfPrice, strangleOpenPrice;
        public double callStrike, putStrike;
        public DateTime closeDate;
        public double closePrice;
    }
}
