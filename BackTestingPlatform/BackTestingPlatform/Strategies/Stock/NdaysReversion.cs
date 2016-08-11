using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Position;

namespace BackTestingPlatform.Strategies.Stock
{
    class NdaysReversion
    {
        public double[] stg(DateTime startDate, DateTime nowDate, AccountInfo account)
        {
            KLinesDataRepository repo = Platforms.container.Resolve<KLinesDataRepository>();

            //策略参数
            //---------------------------------
            int minutePeriod = 1;
            int Ndays= 6;//测试参数,1min数据NDays反转
            int obsAtLeast = Ndays+1;//最少所需样本数，若少于该数，直接返回

            //---------------------------------

            var StockData = repo.fetchFromWind("000001.SH", startDate, nowDate);

            int tradeSignal = 0;//交易信号，1为long，-1为short，无信号为0
            int None = Constants.NONE;//空值
            double[] tradeInfo = new double[3];//存放交易信号信息，用于返回
            double[] priceSeries = new double[StockData.Count];
            DateTime[] dateList = new DateTime[StockData.Count];
            double[] index = new double[StockData.Count];

            //tradeInfo初始化
            tradeInfo[0] = tradeSignal;
            tradeInfo[1] = None;
            tradeInfo[2] = None;

            //取出收盘价
            for (int i = 0; i < StockData.Count; i++)
            {
                priceSeries[i] = StockData[i].close;
                dateList[i] = StockData[i].time;
            }

            //若样本数少于最少所需数量，直接返回
            if (priceSeries.Length < obsAtLeast)
                return tradeInfo;

            //指标计算
            
            


            //生成交易信号
            int dataLen = priceSeries.Length - 1;//最后一个数据的索引


            Console.WriteLine("Time:{0},CP:{1,5:F3},Index:{2,5:F3}", dateList[dataLen], priceSeries[dataLen], index[dataLen]);

            if (priceSeries[dataLen] > index[dataLen] && priceSeries[dataLen - 1] < index[dataLen - 1] && account.positionStatus == 0)
                tradeSignal = 1;//金叉且空仓开多
            else if (priceSeries[dataLen] < index[dataLen] && priceSeries[dataLen - 1] > index[dataLen - 1] && account.positionStatus == 1)
                tradeSignal = -1;//死叉且持仓平多
            else
                tradeSignal = 0;

            tradeInfo[0] = tradeSignal;//交易信号
            tradeInfo[1] = priceSeries[dataLen];//返回实时行情           
            tradeInfo[2] = 1;//初始以1手为基本交易量

            return tradeInfo;
        }
    }
}
