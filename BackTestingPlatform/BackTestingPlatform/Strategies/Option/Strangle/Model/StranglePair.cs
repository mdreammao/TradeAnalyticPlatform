using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.Strangle.Model
{
    public class StranglePair
    {
        public string callCode, putCode;
        public double callPosition, putPosition;
        public DateTime endDate, modifiedDate;
        public double etfPrice, strangleOpenPrice;
        public double callStrike,putStrike;
        public DateTime closeDate;
        public double closePrice;
    }
  }
