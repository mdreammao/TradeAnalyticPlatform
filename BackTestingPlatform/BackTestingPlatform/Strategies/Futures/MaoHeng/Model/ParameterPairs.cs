using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng.Model
{
    /// <summary>
    /// 包含时间的参数对
    /// </summary>
    public class ParameterPairs
    {
        public DateTime tradeday; 
        public int frequency;
        public int numbers;
        public double lossPercent;
        public double ERRatio;
    }

    /// <summary>
    /// 不含时间的参数对
    /// </summary>
    public class FourParameterPairs
    {
        public int frequency;
        public int numbers;
        public double lossPercent;
        public double ERRatio;
    }
}
