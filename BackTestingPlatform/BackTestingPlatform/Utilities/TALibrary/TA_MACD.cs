using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TicTacTec.TA.Library.Core;

namespace BackTestingPlatform.Utilities.TALibrary
{
    public static class TA_MACD
    {
        public static void compute(double[] data, int[] parameters,out double[] dataMACD,out double[] dataMACDSignal,out double[] dataMACDHist)
        {

            RetCode retCode = new RetCode();
            retCode = RetCode.InternalError;

            dataMACD = new double[data.Length];
            dataMACDSignal = new double[data.Length];
            dataMACDHist = new double[data.Length];

            int fastPeriod = parameters[0];
            int slowPeriod = parameters[1];
            int signalPeriod = parameters[2];
            float[] dataSeries = Array.ConvertAll(data,d=>(float)d);

            int outBegIdx = -1;
            int outNbElement = -1;
            int lookback = -1;
            double[] outMACD = new double[data.Length];
            double[] outMACDSignal = new double[data.Length];
            double[] outMACDHist = new double[data.Length];
            lookback = MacdLookback(fastPeriod, slowPeriod, signalPeriod);
            retCode = Macd(0, data.Length - 1, dataSeries, fastPeriod, slowPeriod, signalPeriod, out outBegIdx, out outNbElement, outMACD, outMACDSignal,outMACDHist);
            Array.Copy(outMACD, 0, dataMACD, lookback, data.Length - lookback );
            Array.Copy(outMACDSignal, 0, dataMACDSignal, lookback, data.Length - lookback);
            Array.Copy(outMACDHist, 0, dataMACDHist, lookback, data.Length - lookback);
             
            
        }
    }
}
