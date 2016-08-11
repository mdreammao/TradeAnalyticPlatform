using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    public struct StockDailyInfo : Sequential
    {
        public DateTime time { get; set; }
        public string code { get; set; }
        
        public double dividend;//分红数额
        public double adjFactor;//权息调整比例，现价*该因子为后复权价格，从万德读取
   
    }
}
