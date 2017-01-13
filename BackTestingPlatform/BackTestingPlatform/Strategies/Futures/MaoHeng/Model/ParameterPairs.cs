using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng.Model
{
    /// <summary>
    /// 参数对：时间，四个参数（frequency，numbers，lossPercent，ERRatio）
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
    /// 参数对：时间，分数，四个参数（frequency，numbers，lossPercent，ERRatio）
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

    /// <summary>
    /// 参数对：时间，五个参数（frequency，numbers，lossPercent，longER，shortER）
    /// </summary>
    public class ParaPairs
    {
        public DateTime tradeday { get; set; }
        public int frequency { get; set; }
        public int numbers { get; set; }
        public double lossPercent { get; set; }
        public double longER { get; set; }
        public double shortER { get; set; }
    }

    /// <summary>
    /// 参数对：时间，分数，五个参数（frequency，numbers，lossPercent，longER，shortER）
    /// </summary>
    public class ParaPairsWithScore
    {
        public DateTime tradeday { get; set; }
        public int frequency { get; set; }
        public int numbers { get; set; }
        public double lossPercent { get; set; }
        public double longER { get; set; }
        public double shortER { get; set; }
        public double Score { get; set; }

    }

    /// <summary>
    /// 参数对：四个参数（frequency，numbers，lossPercent，ERRatio）
    /// </summary>
    public class FourParameterPairs
    {
        public int frequency;
        public int numbers;
        public double lossPercent;
        public double ERRatio;
    }

    /// <summary>
    /// 参数对：五个参数（frequency，numbers，lossPercent，longER，shortER）
    /// </summary>
    public class FiveParameterPairs
    {
        public int frequency;
        public int numbers;
        public double lossPercent;
        public double longER;
        public double shortER;
    }


}
