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
        public string industry;
        public string sector;
        //public double[] industry;//行业
        //public double[] sector;//板块
        public double dividend;//分红数额
        public double Factor;//权息调整比例，现价*该因子为后复权价格，从万德读取
        public List<Tick> ticks;
        public List<Quote> kLines;
        public List<Position> postions;
    }

     //stockinfowithtickdata:info
     //   stockinfodaily:info
     //   stockinfowithminutedata:info
    //public class stockBasicinfo

    /// <summary>
    /// 股票Tick价格的格式，每笔成交的价量
    /// </summary>
    //public class StockTickData
    //{

    //    public int code;
    //    public int date, time;//SQL中以整型存放
    //    public double lastPrice;
    //    public Position[] ask, bid;
    //    public double preClose;
    //}

    //public class StockMinuteData
    //{
    //    public DateTime time { get; set; }
    //    public double open { get; set; }
    //    public double high { get; set; }
    //    public double low { get; set; }
    //    public double close { get; set; }
    //    public double volume { get; set; }
    //    public double amount { get; set; }
    //}






}
