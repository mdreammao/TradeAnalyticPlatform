using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Position
{
    /// <summary>
    /// 单只股票持仓数据结构，每只股票的持仓情况，定义每只的数据结构，用List型声明
    /// </summary>
    public class AccountInfo
    {
        public int AccountID;//账户ID
        public int positionStatus;
        public List<StockHolding> myHolding = new List<StockHolding>();//存放持仓股票列表
        public double totalAsset;//总资产
        public double currentEquity;//当前总权益
        public double freeCash;//可用资金
        public double totalProfitLossRate;//总盈亏比率
        public double totalProfitLossAmt;//总盈亏额

        //尚应添加委托单信息
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
    /// 存放历史的交易信息，与AccountInfo用AccountID关联
    /// </summary>
    public class AccountHistory
    {
        public int AccountID;//账户ID
        DateTime tradeTime;//交易时间
        public List<StockHolding> myHolding = new List<StockHolding>();//存放持仓股票列表
        public double totalAsset;//总资产
        public double currentEquity;//当前总权益    
    }
}
