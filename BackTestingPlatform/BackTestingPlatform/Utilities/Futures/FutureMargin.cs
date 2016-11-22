using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.Futures
{
    public class FutureMargin
    {
        /// <summary>
        /// 期货开仓保证金的计算
        /// </summary>
        /// <param name="underlyingOpenPrice">标的开仓价格</param>
        /// <param name="marginRatio">保证金比例</param>
        /// <param name="multiplier">乘数</param>
        /// <returns></returns>
        public static double ComputeOpenMargin(double underlyingOpenPrice,double marginRatio,double multiplier)
        {
            return underlyingOpenPrice * multiplier * marginRatio;
        }

        /// <summary>
        /// 计算维持保证金
        /// </summary>
        /// <param name="underlyingSettlePrice">标的结算价</param>
        /// <param name="marginRatio">保证金比例</param>
        /// <param name="multiplier">乘数</param>
        /// <param name="underlyingOpenPrice">开仓时标的价格</param>
        /// <param name="longOrShort">多空方向</param>
        /// <param name="floatingPnL">结算时浮动盈亏</param>
        /// <returns></returns>
        public static double ComputeMaintenanceMargin(double underlyingSettlePrice,double marginRatio,double multiplier,double underlyingOpenPrice,double longOrShort,ref double floatingPnL)
        {
            double openMargin = ComputeOpenMargin(underlyingOpenPrice, marginRatio, multiplier);
            double PnLBeforeSettle = longOrShort * (underlyingSettlePrice - underlyingOpenPrice)*multiplier;
            double maintenanceMargin= ComputeOpenMargin(underlyingSettlePrice, marginRatio, multiplier);
            floatingPnL = openMargin + PnLBeforeSettle - maintenanceMargin;
            return maintenanceMargin;
        }
        
    }
}
