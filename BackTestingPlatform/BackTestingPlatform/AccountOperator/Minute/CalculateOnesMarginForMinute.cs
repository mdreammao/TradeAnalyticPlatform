using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Utilities.TimeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.AccountOperator.Minute
{
    public static class CalculateOnesMarginForTick
    {
        public static double calculateOnesMargin(string code, double volume, DateTime now, ref Dictionary<string, List<KLine>> data)
        {
            double totalMargin = 0;
            double marginRatio = 0.25;
            //粗糙用法，暂时用开盘价替代前一天的settlementPrice
            if (volume < 0)
                totalMargin += Math.Abs(volume) * data[code].First().open * marginRatio;
            return totalMargin;

        }
    }
}
