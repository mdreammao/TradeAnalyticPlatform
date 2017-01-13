using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{
    public struct OptionGreek
    {
        public string code { get; set; }
        public double coordinate { get; set; }
        public double lastPrice { get; set; }
        public double ask { get; set; }
        public double bid { get; set; }
        public double askv { get; set; }
        public double bidv { get; set; }
        public double impv { get; set; }
        public double delta { get; set; }
        public double gamma { get; set; }
        public double vega { get; set; }
        public double theta { get; set; }
        public double duration { get; set; }

    }
}
