using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Monitor.Model
{
    public class Level1Data
    {
        public double last { get; set; }
        public double ask1 { get; set; }
        public double askv1 { get; set; }
        public double bid1 { get; set; }
        public double bidv1 { get; set; }
        public TimeSpan time { get; set; }
    }
}
