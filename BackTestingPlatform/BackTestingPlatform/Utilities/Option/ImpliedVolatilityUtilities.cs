using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.Option
{
    public class ImpliedVolatilityUtilities
    {
        public static double ComputeOptionPrice(double strike, double duration, double riskFreeRate, double StockRate, string optionType, double optionVolatility, double underlyingPrice)
        {
            double etfPirce = underlyingPrice * Math.Exp(-StockRate * duration);
            return optionLastPrice(etfPirce, optionVolatility, strike, duration, riskFreeRate, optionType);
        }
        public static double ComputeImpliedVolatility(double strike,double duration,double riskFreeRate,double StockRate,string optionType,double optionPrice,double underlyingPrice)
        {
            double etfPirce = underlyingPrice * Math.Exp(-StockRate * duration);
            return sigma(etfPirce, optionPrice, strike, duration, riskFreeRate, optionType);
        }
        public static double _StartPoint(double K, double T, double r, double call, double s)///K 是 执行价格 
        {
            double sigma = 0.5;
            double x = K * Math.Exp(-r * T); ///x是折现值
            double radicand = Math.Pow(call - (s - x) / 2, 2) - Math.Pow(s - x, 2) / Math.PI * (1 + x / s) / 2;
            if (radicand>0)
            {
                sigma = 1 / Math.Sqrt(T) * Math.Sqrt(2 * Math.PI) / (s + x) * (call - (s - x) / 2 + Math.Sqrt(radicand));
            }
            return sigma;
        }
        /// <summary>
        /// 计算看涨期权隐含波动率。利用简单的牛顿法计算期权隐含波动率。在计算中，当sigma大于3，认为无解并返回0
        /// </summary>
        /// <param name="callPrice">期权价格</param>
        /// <param name="spotPrice">标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回隐含波动率</returns>
        public static double sigmaOfCall(double callPrice, double spotPrice, double strike, double duration, double r)
        {
            double sigma =_StartPoint(strike,duration,r,callPrice,spotPrice), sigmaold = sigma;
           if (callPrice < spotPrice - strike * Math.Exp(-r * duration))
           {
                return 0;
           }
            for (int num = 0; num <= 10; num++)
            {
                sigmaold = sigma;
                double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
                double d2 = d1 - sigma * Math.Sqrt(duration);
                double f_sigma = normcdf(d1) * spotPrice - normcdf(d2) * strike * Math.Exp(-r * duration);
                double df_sigma = spotPrice * Math.Sqrt(duration) * Math.Exp(-d1 * d1 / 2) / (Math.Sqrt(2 * Math.PI));
                if (df_sigma<=0.000001)
                {
                    break;  
                }
                sigma = sigma + (callPrice - f_sigma) / df_sigma;
                if (Math.Abs(sigma - sigmaold) < 0.0001)
                {
                    break;
                }
            }
            if (sigma < 0)///sigma > 3 ||
            {
                sigma = 0;
            }
           
            return sigma;
        }

        /// <summary>
        /// 计算看跌期权隐含波动率。利用简单的牛顿法计算期权隐含波动率。在计算中，当sigma大于3，认为无解并返回0
        /// </summary>
        /// <param name="callPrice">期权价格</param>
        /// <param name="spotPrice">标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回隐含波动率</returns>
        private static double sigmaOfPut(double putPrice, double spotPrice, double strike, double duration, double r)
        {
            return sigmaOfCall(putPrice + spotPrice - strike * Math.Exp(-r * duration), spotPrice, strike, duration, r); 
        }

        /// <summary>
        /// 利用期权价格等参数计算隐含波动率
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="optionLastPrice">期权价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="duration">期权到日期</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型区分看涨还是看跌</param>
        /// <returns>返回隐含波动率</returns>
        public static double sigma(double etfPrice, double optionLastPrice, double strike, double duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return sigmaOfCall(optionLastPrice, etfPrice, strike, duration, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return sigmaOfPut(optionLastPrice, etfPrice, strike, duration, r);
            }
            return 0;
        }


        /// <summary>
        /// 根据隐含波动率计算期权价格
        /// </summary>
        /// <param name="etfPrice">50etf价格</param>
        /// <param name="sigma">隐含波动率</param>
        /// <param name="strike">期权行权价格</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <param name="optionType">期权类型看涨还是看跌</param>
        /// <returns>返回期权理论价格</returns>
        public static double optionLastPrice(double etfPrice, double sigma, double strike, double duration, double r, string optionType)
        {
            if (optionType.Equals("认购"))
            {
                return callPrice(etfPrice, strike, sigma, duration, r);
            }
            else if (optionType.Equals("认沽"))
            {
                return putPrice(etfPrice, strike, sigma, duration, r);
            }
            return 0.0;
        }


        /// <summary>
        /// 计算看涨期权理论价格
        /// </summary>
        /// <param name="spotPrice">期权标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="sigma">期权隐含波动率</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回看涨期权理论价格</returns>
        private static double callPrice(double spotPrice, double strike, double sigma, double duration, double r)
        {
            if (duration == 0)
            {
                return ((spotPrice - strike) > 0) ? (spotPrice - strike) : 0;
            }
            double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
            double d2 = d1 - sigma * Math.Sqrt(duration);
            return normcdf(d1) * spotPrice - normcdf(d2) * strike * Math.Exp(-r * duration);
        }

        /// <summary>
        /// 计算看跌期权理论价格
        /// </summary>
        /// <param name="spotPrice">期权标的价格</param>
        /// <param name="strike">期权行权价</param>
        /// <param name="sigma">期权隐含波动率</param>
        /// <param name="duration">期权到期日</param>
        /// <param name="r">无风险利率</param>
        /// <returns>返回看跌期权理论价格</returns>
        private static double putPrice(double spotPrice, double strike, double sigma, double duration, double r)
        {
            if (duration == 0)
            {
                return ((strike - spotPrice) > 0) ? (strike - spotPrice) : 0;
            }
            double d1 = (Math.Log(spotPrice / strike) + (r + sigma * sigma / 2) * duration) / (sigma * Math.Sqrt(duration));
            double d2 = d1 - sigma * Math.Sqrt(duration);
            return -normcdf(-d1) * spotPrice + normcdf(-d2) * strike * Math.Exp(-r * duration);
        }

        /// <summary>
        /// 辅助函数erf(x),利用近似的方法进行计算
        /// </summary>
        /// <param name="x">因变量x</param>
        /// <returns>返回etf(x)</returns>
        private static double erf(double x)
        {
            double tau = 0;
            double t = 1 / (1 + 0.5 * Math.Abs(x));
            tau = t * Math.Exp(-Math.Pow(x, 2) - 1.26551223 + 1.00002368 * t + 0.37409196 * Math.Pow(t, 2) + 0.09678418 * Math.Pow(t, 3) - 0.18628806 * Math.Pow(t, 4) + 0.27886807 * Math.Pow(t, 5) - 1.13520398 * Math.Pow(t, 6) + 1.48851587 * Math.Pow(t, 7) - 0.82215223 * Math.Pow(t, 8) + 0.17087277 * Math.Pow(t, 9));
            if (x >= 0)
            {
                return 1 - tau;
            }
            else
            {
                return tau - 1;
            }
        }

        /// <summary>
        /// 辅助函数normcdf(x)
        /// </summary>
        /// <param name="x">因变量x</param>
        /// <returns>返回normcdf(x)</returns>
        private static double normcdf(double x)
        {
            return 0.5 + 0.5 * erf(x / Math.Sqrt(2));
        }
    }
}
