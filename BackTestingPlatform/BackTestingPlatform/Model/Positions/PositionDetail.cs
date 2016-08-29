using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class PositionDetail
    {
        public double volume { get; set; }
        public double totalCost { get; set; }
        public double averagePrice { get; set; }
    }
}
