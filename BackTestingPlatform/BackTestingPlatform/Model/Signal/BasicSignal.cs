using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Signal
{
    public class BasicSignal
    {
        public DateTime time { get; set; }
        public string code { get; set; }
        public double positions { get; set; }
        public double price { get; set; }
        public string type { get; set; }
    }
}
