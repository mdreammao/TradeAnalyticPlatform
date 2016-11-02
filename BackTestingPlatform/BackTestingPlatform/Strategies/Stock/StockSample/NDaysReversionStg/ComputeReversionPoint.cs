using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Transaction;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Stock.StockSample01
{
    public static class ComputeReversionPoint
    {
        /// <summary>
        /// 计算向上的反转点，数据长度小于回望周期时为0，代表空值
        /// </summary>
        /// <param name="dataSeries"></param>数据序列，KLine格式
        /// <param name="Ndays"></param>
        /// <param name="lengthOfBackLooking"></param>
        /// <returns></returns>
        public static List<double> findUpReversionPoint(List<KLine> dataSeries, int Ndays, int lengthOfBackLooking)
        {
            List<double> indexList = new List<double>();
            for (int i = 0; i < dataSeries.Count; i++)
            {
                if (i < lengthOfBackLooking - 1)
                {
                    indexList.Add(0);//空值记为0
                    continue;
                }
                else
                {
                    KLine lowestPoint = new KLine();
                    // var itemOfLowestPoint = dataSeries.Where((item, index) => index <= i && index >= i - lengthOfBackLooking - 1).Select((m, index) => new { index, m }).OrderByDescending(n => -n.m.low).Take(1).ToList();
                    var itemOfLowestPoint = dataSeries.GetRange(i - (lengthOfBackLooking - 1), lengthOfBackLooking).OrderByDescending(n => -n.low).Take(1).ToList();
                    lowestPoint = itemOfLowestPoint[0];
                    int indexOfLowestPoint = dataSeries.FindIndex(x => x.time == lowestPoint.time);
                    if (indexOfLowestPoint - Ndays < 0)
                        indexList.Add(0);//空值记为0
                    else
                    {
                        //根据周期确定分钟k线上的起止索引，求起止区间的最大值
                      //  int index1 = 
                        indexList.Add(dataSeries[indexOfLowestPoint - Ndays].high);
                    }                       
                }

            }

            return indexList;

        }

        /// <summary>
        /// 计算向下的反转点，数据长度小于回望周期时为0，代表空值
        /// </summary>
        /// <param name="dataSeries"></param>
        /// <param name="Ndays"></param>
        /// <param name="lengthOfBackLooking"></param>
        /// <returns></returns>
        public static List<double> findDownReversionPoint(List<KLine> dataSeries, int Ndays, int lengthOfBackLooking)
        {
            List<double> indexList = new List<double>();
            for (int i = 0; i < dataSeries.Count; i++)
            {
                if (i < lengthOfBackLooking - 1)
                {
                    indexList.Add(0);//空值记为0
                    continue;
                }
                else
                {
                    KLine highestPoint = new KLine();
                    //var itemOfHighestPoint = dataSeries.Where((item, index) => index <= i && index >= i - lengthOfBackLooking - 1).Select((m, index) => new { index, m }).OrderByDescending(n => n.m.high).Take(1).ToList();
                    var itemOfHighestPoint = dataSeries.GetRange(i - (lengthOfBackLooking - 1), lengthOfBackLooking).OrderByDescending(n => n.high).Take(1).ToList();
                    highestPoint = itemOfHighestPoint[0];
                    int indexOfHighestPoint = dataSeries.FindIndex(x => x.time == highestPoint.time);
                    if (indexOfHighestPoint - Ndays < 0)
                        indexList.Add(0);//空值记为0
                    else
                        indexList.Add(dataSeries[indexOfHighestPoint - Ndays].low);
                }

            }

            return indexList;


        }


    }
}
