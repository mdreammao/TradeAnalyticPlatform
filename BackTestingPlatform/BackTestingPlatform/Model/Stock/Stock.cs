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
        public string stockCode { get; set; }
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
    /// 单只股票持仓数据结构，每只股票的持仓情况，定义每只的数据结构，用List型声明
    /// </summary>
    /// 
    public class StockHolding
    {
        public string stockCode { get; set; }
        public int volume;
        public double aveCost;
        public double nowPrice;
        public double profitLossRate;
        public double profitLossAmt;
        public int positionDirection;//long(1) & short(-1)
        public double freeze;//冻结仓位
        public double margin;//保证金
        public DateTime entryTime;

    }

    /// <summary>
    /// 单只股票持仓数据结构，每只股票的持仓情况，定义每只的数据结构，用List型声明
    /// </summary>
    public class AccountInfo
    {
        public int AccountID;//账户ID
        public List<StockHolding> myHolding = new List<StockHolding>();//存放持仓股票列表
        public double totalAsset;//总资产
        public double currentEquity;//当前总权益
        public double freeCash;//可用资金
        public double totalProfitLossRate;//总盈亏比率
        public double totalProfitLossAmt;//总盈亏额

        //尚应添加委托单信息
    }
}
