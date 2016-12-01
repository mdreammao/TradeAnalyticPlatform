using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Futures.MaoHeng
{
    /// <summary>
    /// Perry Kaufman在他 1995年的着作 " Smarter Trading"首次提出了效率比值 （ER）。它是一种趋势强度的衡量,用以调整市场的波动程度.它的计算方法如下 Efficiency Ratio = direction / volatility ER = （N 期间内价格总变化的绝对值）/ （N 期间内个别价格变化的绝对值） 如果个别价格变化都是正值 （或负值），那么 ER 将等于 1.0，这代表了强劲的趋势行情。然而，如果有正面和负面价格变动造成相互的抵消，代表公式中的分子将会缩小，ER 将会减少。ER 反映价格走势的一致性。ER 的所有值将都介于 0.0 ~ 1.0  另外一种计算方式为ER = （N 期间内价格总变化）/ （N 期间内个别价格变化的绝对值）此时 ER值的变化范围就会落在 -1.0 ~ 1.0 之间 , 分别代表涨势与跌势的方向 , 其中 0 代表无方向性的波动
    /// </summary>
    public class EfficiencyRatio
    {
        //回测参数设置
        private double initialCapital = 25000;
        private double optionVolume = 10000;
        private double slipPoint = 0.001;
        private DateTime startDate, endDate;
        private string underlying;
        private int frequency=1;
        /// <summary>
        /// 策略的构造函数
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public EfficiencyRatio(int startDate, int endDate,string underlying,int frequency)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            this.underlying = underlying;
            this.frequency = frequency;
        }
        private  void getData(DateTime today,string code)
        {
            var data = Platforms.container.Resolve<FuturesMinuteRepository>().fetchFromLocalCsvOrWindAndSave(code, today);

        }
    }
}
