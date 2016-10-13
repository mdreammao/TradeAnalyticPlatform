using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TicTacTec.TA.Library.Core;

namespace BackTestingPlatform.Model.TALibrary
{
    public static class TA_SMA
    {
        public static double[] SMA(double[] data, int length)
        {
            double[] indexValue = new double[data.Length];

            RetCode retCode = new RetCode();
            retCode = RetCode.InternalError;

            int outBegIdx = -1;
            int outNbElement = -1;
            int lookback = -1;
            double[] output = new double[data.Length];
            lookback = MovingAverageLookback(length, MAType.Sma);
            retCode = MovingAverage(0, data.Length - 1, data, lookback + 1, MAType.Sma, out outBegIdx, out outNbElement, output);
            Array.Copy(output, 0, indexValue, length - 1, output.Length - (length - 1));

            return indexValue;
            
        }

    }
}
