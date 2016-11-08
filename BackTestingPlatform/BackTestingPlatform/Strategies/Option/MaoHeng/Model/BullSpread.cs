using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.MaoHeng.Model
{
    public class BullSpread
    {
        public string code1, code2;
        public double strike1, strike2;
        public DateTime endDate,spreadOpenDate;
        public double etfPrice_Open, spreadPrice_Open;
    }
}
