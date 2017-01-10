using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Monitor.Option.Model
{
    public class LongDeltaPair
    {
        public string option1 { get; set; }
        public string option2 { get; set; }
        public double profit { get; set; }
        public double loss { get; set; }
        public double mark { get; set; }
    }
}
