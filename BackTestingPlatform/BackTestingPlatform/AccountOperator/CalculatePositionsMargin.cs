using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Utilities.TimeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Positions
{
    public static class CalculatePositionsMargin
    {
        /// <summary>
        /// 根据当前持仓计算保证金，
        /// </summary>
        /// <param name="nowPosition"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        public static double calculateMargin(Dictionary<string, PositionsWithDetail> nowPosition, DateTime now, ref Dictionary<string, List<KLine>> data)
        {
            double totalMargin = 0;
            
            foreach (var position0 in nowPosition.Values)
            {
                //若当前持有空头头寸，则计算保证金
                if (position0.volume < 0)
                {
                    //若当前持仓品种为期权
                    if (position0.tradingVarieties.Equals("option"))
                    {                       
                        totalMargin += Math.Abs(position0.volume)*data[position0.code].First().close;
                    }
                }
            }
            totalMargin *= 0.25;

            return totalMargin;

        }
    }
}
