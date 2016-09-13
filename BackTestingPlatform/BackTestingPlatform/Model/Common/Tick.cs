using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{

    public class Tick : Sequential, ICloneable
    {
        public string code { get; set; }
        public DateTime time { get; set; }
        public double lastPrice { get; set; }
        public Position[] ask { get; set; }
        public Position[] bid { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
        public override string ToString()
        {
            return String.Format("t={0},code={1},lastp={2}", time, code, lastPrice);
        }
    }

    public class TickFromMssql : Tick
    {
        public double high { get; set; }
        public double low { get; set; }
       
        public double volume { get; set; }
        public double amount { get; set; }
        public double preSettle { get; set; }

        public double preClose { get; set; }

        public int date;

        public int moment;

        
    }
}
