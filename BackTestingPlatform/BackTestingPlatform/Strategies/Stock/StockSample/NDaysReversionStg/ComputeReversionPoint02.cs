using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Stock.StockSample.NDaysReversionStg
{
    public static class ComputeReversionPoint02
    {
        /// <summary>
        /// 计算向上的反转点，数据长度小于回望周期时为0，代表空值
        /// </summary>
        /// <param name="dataSeries"></param>数据序列，KLine格式,可能是任意频率
        /// <param name="Ndays"></param>
        /// <param name="lengthOfBackLooking"></param>
        /// <returns></returns>
        public static double findUpReversionPoint(List<KLine> dataSeries, int indexOfNow, int Ndays, int lengthOfBackLooking)
        {
            double upReversionPoint = 0;
            var tempData = dataSeries.GetRange(indexOfNow - lengthOfBackLooking + 1, lengthOfBackLooking);
            var lowestPoint = tempData.Select((m, index) => new { m, index }).OrderBy(n => n.m.low).Take(1);
            int indexOfLowPoint = lowestPoint.Select(n => n.index).First();
            if (indexOfLowPoint < Ndays)
                return -1;
            else
            {
                upReversionPoint = dataSeries[indexOfLowPoint - Ndays].high;
                return upReversionPoint;
            }

        }

        /// <summary>
        /// 计算向下的反转点，数据长度小于回望周期时为0，代表空值
        /// </summary>
        /// <param name="dataSeries"></param>
        /// <param name="Ndays"></param>
        /// <param name="lengthOfBackLooking"></param>
        /// <returns></returns>
        public static double findDownReversionPoint(List<KLine> dataSeries, int indexOfNow, int Ndays, int lengthOfBackLooking)
        {
            double downReversionPoint = 0;
            var tempData = dataSeries.GetRange(indexOfNow - lengthOfBackLooking + 1, lengthOfBackLooking);
            var highestPoint = tempData.Select((m, index) => new { m, index }).OrderByDescending(n => n.m.high).Take(1);
            int indexOfHighPoint = highestPoint.Select(n => n.index).First();
            if (indexOfHighPoint < Ndays)
                return -1;
            else
            {
                downReversionPoint = dataSeries[indexOfHighPoint - Ndays].high;
                return downReversionPoint;
            }
        }

    }
}
