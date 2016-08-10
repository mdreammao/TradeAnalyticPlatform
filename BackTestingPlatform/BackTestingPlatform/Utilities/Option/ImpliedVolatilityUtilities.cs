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
        public static double ComputeOptionPrice()
        {
            return 0;
        }
        public static double ComputeImpliedVolatility(string optionCode,double strike,double duration,double riskFreeRate,double StockRate,string optionType,double optionPrice,double underlyingPrice)
        {
            double sigma0 = 0;
            if (optionType=="认购")
            {

            }
            else
            {

            }
            return 0.0;
        }
        private double _StartPoint(double K, double T, double r, double call, double s)
        {
            double sigma = 0.0;
            double x = K * Math.Exp(-r * T);
            double radicand = Math.Pow(call - (s - x) / 2, 2) - Math.Pow(s - x, 2) / Math.PI * (1 + x / s) / 2;
            if (radicand>0)
            {
                sigma = 1 / Math.Sqrt(T) * Math.Sqrt(2 * Math.PI) / (s + x) * (call - (s - x) / 2 + Math.Sqrt(radicand));
            }
            return sigma;
        }
    }
}
