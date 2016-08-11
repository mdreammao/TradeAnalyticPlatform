using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BackTestingPlatform.Model.Stock
{
    public class StockDaily: Kline
    {
        public string code;//股票代码
        public string name;
        public DateTime date;//当前时间
        public DateTime ipoDate;//上市时间
        public bool isST;//是否是Special Treament股
        public string industries;//行业
        public string sectors;//板块，多板块的用;隔开，如"石墨烯;一带一路"
        public string province;//公司所在省份
        public double adjustedFactor;//权息调整比例，现价*该因子为后复权价格，从万德读取
        public double marketValue;//总市值
        public double marketValueFloat;//流通市值
        public double turnover;//换手率
        public string tradeStatus;//"交易"、"停牌"

    }
}
