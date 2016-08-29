using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class BasicAccount
    {
        public DateTime time { get; set; }
        public double totalAssets { get; set; }
        public double freeCash { get; set; }
        public double positionValue { get; set; }
        public double margin { get; set; }
    }
}
