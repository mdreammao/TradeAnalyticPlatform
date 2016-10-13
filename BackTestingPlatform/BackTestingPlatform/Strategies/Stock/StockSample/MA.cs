using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Stock.StockSample
{
    public static class MA
    {
        public static double[] compute(double[] data, int length)
        {
            double[] MAValue = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (i < length )
                    MAValue[i] = 0;
                else
                {
                    double[] temp = new double[length];
                    Array.Copy(data, i - length, temp, 0, length);
                    MAValue[i] = temp.Average();
                }

            }
            return MAValue;

        }
        
    }
}
