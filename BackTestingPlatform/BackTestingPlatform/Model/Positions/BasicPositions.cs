using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public class BasicPositions
    {
        public string code { get; set; }
        public DateTime time { get; set; }
        public double volume { get; set; }
        public double currentPrice { get; set; }
        public List<TransactionRecord> record { get; set; }
        public double transactionCost { get; set; }
        public double totalCost { get; set; }
        //平均持仓成本，盈亏累积计入此价格
        public double averageCost { get; set; }
        //实时总权益
        public double totalAmt { get; set; }
    }
}
