using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    public struct StockInfo
    {
        //股票代码
        public string code { get; set; }
        //股票名称
        public string name { get; set; }
        //当前时间
        public DateTime time { get; set; }
        //上市时间
        public DateTime ipoDate { get; set; }
        //是否是Special Treament股
        public bool isST { get; set; }
        //行业
        public string industries { get; set; }
        //板块，多板块的用;隔开，如"石墨烯;一带一路"
        public string sectors { get; set; }
        //公司所在省份
        public string province { get; set; }
        //权息调整比例，现价*该因子为后复权价格，从万德读取
        public double adjustedFactor { get; set; }
        //后复权因子,实时股价*backfwdFactor为后复权价格   
        public double backfwdFactor { get; set; }
        //总市值
        public double marketValue { get; set; }
        //流通市值
        public double marketValueFloat { get; set; }
        //换手率
        public double turnover { get; set; }
        //"交易"、"停牌"
        public string tradeStatus { get; set; }
    }
}
