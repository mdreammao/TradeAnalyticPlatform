using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{
    public static class Constants
    {

        public static TimeLine timeline500ms = new TimeLine(
                new TimeLineSection("09:30:00.000", "11:30:00.000", 500),
                new TimeLineSection("13:00:00.000", "15:00:00.000", 500)
                );
    }
}
