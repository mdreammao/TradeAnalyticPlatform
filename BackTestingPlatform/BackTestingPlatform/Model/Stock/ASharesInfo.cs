using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    /// <summary>
    /// 所有A股基本信息的数据结构
    /// </summary>
    public class ASharesInfo
    {
        public string stockCode;
        public string stockName;
        public double recentClosePrice;
        public double recentTotalMarketValue;
        public double recentMarketCapFloat;
        public string recentStatus;
        public DateTime lastTradeDay;
        public DateTime IPODate;
        public string province;
        public string type;
        public string board;
        public string exchange;
    }
}
