using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model
{
    /// <summary>
    /// 股票基础信息的格式，包括上市时间，权息信息等
    /// </summary>
    public class StockInfo
    {
        public int code;//股票代码
        public DateTime timeToMarket;//上市时间
        public bool isST;//是否是Special Treament股
        public double[] industry;//行业
        public double[] sector;//板块
        public double dividend;//分红数额
        public double adjFactor;//权息调整比例，现价*该因子为后复权价格，从万德读取
        public List<StockTickData> ticks;
        public List<KLinesData> kLines;
        public List<PositionData> postions;
    }

    /// <summary>
    /// 股票Tick价格的格式，每笔成交的价量
    /// </summary>
    public class StockTickData
    {

        public int code;
        public int date, time;//SQL中以整型存放
        public double lastPrice;
        public PositionData[] ask, bid;
        public double preClose;
    }

    public class StockMinuteData
    {
        public DateTime time { get; set; }
        public double open { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double close { get; set; }
        public double volume { get; set; }
        public double amount { get; set; }
    }

    /// <summary>
    /// 股票K线价格的格式，开高低收量额
    /// </summary>
    public class KLinesData
    {
        //public string stockCode { get; set; }
        public DateTime time;
        public double open, high, low, close, volume, amount;//开高低收量额

    }

    /// <summary>
    /// 实时行情，对应WindApi中的wsq函数，本地数据库WindFullMarket中的marketData表结构
    /// </summary>
    public class RealTimeQuotes
    {
        public double open, high, low, close, volume, amount;//开高低收量额
        public DateTime time;
        public double cp;
        public double s1, s2, s3, s4, s5;

    }


}
