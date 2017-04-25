using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Monitor.Model
{
    public class CorrStatic
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string underlying1 { get; set; }
        public string underlying2 { get; set; }
        public double corr { get; set; }

    }
}
