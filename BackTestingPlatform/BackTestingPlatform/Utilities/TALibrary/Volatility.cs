using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.TALibrary
{
    /// <summary>
    /// 计算波动率的工具
    /// </summary>
    public static class Volatility
    {
        /// <summary>
        /// 计算历史波动率
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="length">波动率计算周期</param>
        /// <returns>历史波动率</returns>
        public static double[] HVYearly(double[] data,int step)
        {

            int length = data.Length;
            double[] HV = new double[length];
            if (length<=step+1 || step<=1)
            {
                return HV;
            }
            double[] logData = new double[length];
            double[] movingFirstMoment = new double[length];
            double[] movingSecondaryMoment = new double[length];
            for (int i = 1; i < length; i++)
            {
                logData[i] = Math.Log(data[i] / data[i - 1]);
            }
            double movingFirstMoment0 = 0;
            double movingSecondaryMoment0 = 0;
            for (int i = 1; i <= step; i++)
            {
                movingFirstMoment0 += logData[i]/step;
                movingSecondaryMoment0 += logData[i] * logData[i]/step;
            }
            HV[step] = Math.Sqrt(movingSecondaryMoment0 - Math.Pow(movingFirstMoment0, 2));
            for (int i = step+1; i < length; i++)
            {
                movingFirstMoment0 += (logData[i]-logData[i-step])/step;
                movingSecondaryMoment0 += (Math.Pow(logData[i],2)-Math.Pow(logData[i-step],2)) / step;
                HV[i] = Math.Sqrt(movingSecondaryMoment0 - Math.Pow(movingFirstMoment0, 2));
            }
            return HV;
        }
    }
}
