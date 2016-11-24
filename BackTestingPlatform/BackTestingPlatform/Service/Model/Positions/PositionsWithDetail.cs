using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class PositionsWithDetail : BasicPositions
    {
        public PositionDetail LongPosition { get; set; }
    //    public PositionDetail closeLong { get; set; }
        public PositionDetail ShortPosition { get; set; }
    //    public PositionDetail clsoeShort { get; set; }
    }
}
