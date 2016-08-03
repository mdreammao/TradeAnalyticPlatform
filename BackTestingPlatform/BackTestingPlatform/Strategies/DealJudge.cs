using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BackTestingPlatform.Model.TALibrary;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Position;

namespace BackTestingPlatform.Strategies
{
    class DealJudge
    {

        //存放成交回报用于返回，[1]是否成交、[2]成交价、[3]成交量（考虑到追单到极限时间依然部分成交的情况）
     //   public double[] TransactionReturn;

        KLinesDataRepository repo = Platforms.container.Resolve<KLinesDataRepository>();
        /// <summary>
        /// 当出现下单信号时，判断是否成交，返回成交时间、成交价格、成交额
        /// </summary>
        ///     
        public double[] Judge(DateTime nowDate, double[] signalArray )
        {

            double[] transReturn = new double[4];//存放成交回报数据
            //MarketData存放nowDate的行情数据,格式为时间、O、H、L、C、Volume、Amt
            var MarketData = repo.fetchFromWind("510050.SH", nowDate.AddMinutes(1), nowDate.AddMinutes(2));//取得下一根bar的行情数据

            //按VWAP成交
            transReturn[1] = 1;
            transReturn[2] = MarketData[1].amount / MarketData[1].volume;
            transReturn[3] = 1;

            return transReturn;
        
        
        }
        


    }
}
