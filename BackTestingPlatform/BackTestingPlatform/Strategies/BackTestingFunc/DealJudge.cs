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
using BackTestingPlatform.Model.Positions;

namespace BackTestingPlatform.Strategies
{
    class DealJudge
    {

        //存放成交回报用于返回，[1]是否成交、[2]成交价、[3]成交量（考虑到追单到极限时间依然部分成交的情况）
     //   public double[] TransactionReturn;

        KLinesDataRepository repo = Platforms.container.Resolve<KLinesDataRepository>();
        /// <summary>
        /// 当出现下单信号时，判断是否成交，返回成交时间(当前bar几根之后成交,double型,默认为1)、成交价格、成交额
        /// </summary>
        ///     
        public double[] Judge(DateTime nowDate, double[] signalArray )
        {

            double[] transReturn = new double[4];//存放成交回报数据
            //MarketData存放nowDate的行情数据,格式为时间、O、H、L、C、Volume、Amt
          //  var MarketData = repo.fetchFromWind("510050.SH", nowDate.AddMinutes(1), nowDate.AddMinutes(2));//取得下一根bar的行情数据

            //按VWAP成交
            transReturn[0] = 1;
            transReturn[1] = 1;

            if (signalArray[0] == 1)
                transReturn[2] = signalArray[1] * (1 + 0.002);//当前价+0.2%的冲击成本
            else if (signalArray[0] == -1)
                transReturn[2] = signalArray[1] * (1 - 0.002);//当前价+0.2%的冲击成本
            else
                transReturn[2] = signalArray[1];//无信号时返回实时行情

            transReturn[3] = 1;

            return transReturn;
        
        
        }
        


    }
}
