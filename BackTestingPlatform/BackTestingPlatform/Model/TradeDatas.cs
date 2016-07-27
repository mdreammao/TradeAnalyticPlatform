using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model
{
    public class TickData
    {
        public string stockCode { get;set;}
        public DateTime time;
        public double cp;
        
    }

    public class KLinesData
    {
        public string stockCode { get; set; }
        public DateTime time;
        public double open, high, low, close, volume;
         
    }
    

    /// <summary>
    /// 分钟序列数据
    /// </summary>
    public class WsiData
    {
        public string stockCode;
        public DateTime time;
        public double open, high, low, close, volume;
    }
}
