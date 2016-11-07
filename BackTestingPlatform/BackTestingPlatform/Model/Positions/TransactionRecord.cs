using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class TransactionRecord
    {
        public DateTime time { get; set; }
        public double volume { get; set; }
        public double price { get; set; }
        public string code { get; set; }
    }
}
