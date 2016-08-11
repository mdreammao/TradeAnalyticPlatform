using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Common
{
    /// <summary>
    /// 实时报价，包含高开低收量额
    /// </summary>
    public struct Quote : Sequential
    {
        public DateTime time { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public double amount { get; set; }
    }
}
