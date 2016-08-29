using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class PositionsWithDetail : BasicPositions
    {
        public PositionDetail openLong { get; set; }
        public PositionDetail closeLong { get; set; }
        public PositionDetail openShort { get; set; }
        public PositionDetail clsoeShort { get; set; }

    }
}
