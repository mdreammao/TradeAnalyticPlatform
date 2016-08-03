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
        public DateTime timeToMarket;//上市时间
        public bool isST;//是否是Special Treament股
        public double[] industry;//行业
        public double[] sector;//板块
        public double dividend;//分红数额
        public double exRightRatio;//除权比例
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
    /// 股票盘口价格的格式，不定长度的N挡盘口价量
    /// </summary>
    public class PositionData
    {
        public double price, volume;
        public PositionData(double price, double volume)
        {
            this.price = price;
            this.volume = volume;
        }
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
