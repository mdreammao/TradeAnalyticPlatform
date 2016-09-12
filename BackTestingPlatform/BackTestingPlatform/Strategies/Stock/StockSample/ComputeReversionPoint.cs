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
using BackTestingPlatform.Transaction.TransactionWithSlip;
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
                    List<KLine> tempList = dataSeries.Where((item, index) => index <= i && index >= i - lengthOfBackLooking -1 ).ToList();
                    var indexOfLP = tempList.Select((m, index) => new { index, m }).OrderByDescending(n => n.m.low).Take(1);

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
            foreach (var nowData in dataSeries)
            {


            }

            return indexList;


        }

        private static 
    }
}
