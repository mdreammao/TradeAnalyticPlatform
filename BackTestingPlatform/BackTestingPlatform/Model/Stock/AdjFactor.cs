using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Stock
{
    /// <summary>
    /// 股票权息调整因子，对应wind中的adjfactor
    /// 后复权因子，实时价格*adjfactor得到后复权价格，回测均使用后复权数据
    /// </summary>

    public class AdjFactor
    {
        public DateTime time;
        public int code;
        public double backfwdFactor;//后复权因子,实时股价*backfwdFactor为后复权价格   
    }
}
