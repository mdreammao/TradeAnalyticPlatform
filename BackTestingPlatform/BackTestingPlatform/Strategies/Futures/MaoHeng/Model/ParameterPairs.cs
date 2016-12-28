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
        public DateTime tradeday { get; set; }
        public int frequency { get; set; }
        public int numbers { get; set; }
        public double lossPercent { get; set; }
        public double ERRatio { get; set; }
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

    /// <summary>
    /// 包含分数的参数对
    /// </summary>
    public class ParameterPairsWithScore
    {
        public DateTime tradeday { get; set; }
        public int frequency { get; set; }
        public int numbers { get; set; }
        public double lossPercent { get; set; }
        public double ERRatio { get; set; }
        public double Score { get; set; }
    }
}
