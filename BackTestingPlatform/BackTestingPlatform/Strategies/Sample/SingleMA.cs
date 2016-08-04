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

namespace BackTestingPlatform.Strategies.MA
{

    class SingleMA : BackTesting
    {

        /// <summary>
        /// 策略回测，返回[1]成交信号、[2]交易价格、[3]交易量
        /// </summary>
        /// <param name="startDate"></param>回测开始时间
        /// <param name="nowDate"></param>现在时间
        /// <param name="account"></param>当前账户信息
        /// <returns></returns>

        public double[] stg(DateTime startDate, DateTime nowDate, AccountInfo account)
        {
            KLinesDataRepository repo = Platforms.container.Resolve<KLinesDataRepository>();

            //计算运行时间
            /*
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间 

        
            stopwatch.Stop(); //  停止监视 
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double seconds = timespan.TotalSeconds;  //  总秒数
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数
            System.Console.WriteLine("Running Time1:{0} seconds.\nRunning Time2:{1} seconds.", seconds,seconds1);
            System.Console.ReadKey();
            */
            /*
            for (int i = 0; i < StockData.Count; i++)
            {              
                Console.WriteLine("Time:{0,-18} -- O:{1,8:F3} H:{2,8:F3} L:{3,8:F3} C:{4,8:F3} V:{5,8:F0} A:{6,8:F0}",
                    StockData[i].time, StockData[i].high, StockData[i].open, StockData[i].low, StockData[i].close, 
                    StockData[i].volume,StockData[i].amount);
                if (i % 50 == 49)
                {
                    System.Console.WriteLine("--Press any key--");
                    System.Console.ReadKey();
                }
            }
             */

            var StockData = repo.fetchFromWind("510050.SH", startDate, nowDate);
         
            int tradeSignal = 0;//交易信号，1为long，-1为short，无信号为0
            int None = Constants.NONE;//空值
            double[] tradeInfo = new double[3];//存放交易信号信息，用于返回
            double[] priceSeries = new double[StockData.Count];
            DateTime[] dateList = new DateTime[StockData.Count];
            double[] index = new double[StockData.Count];
            int MAParam = 55;//五Bars均线，测试参数
            int obsAtLeast = MAParam;//最少所需样本数，若少于该数，直接返回


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

            //均线指标计算
            TA_MA myMA = new TA_MA(priceSeries);
            index = myMA.SMA(MAParam);

            
            //生成交易信号
            int dataLen = priceSeries.Length - 1;//最后一个数据的索引

            Console.WriteLine("Time:{0},CP:{1,5:F3},Index:{2,5:F3}", dateList[dataLen], priceSeries[dataLen], index[dataLen]);

            if (priceSeries[dataLen] > index[dataLen] && priceSeries[dataLen - 1] < index[dataLen - 1] && account.positionStatus == 0)
                tradeSignal = 1;//金叉且空仓开多
            else if (priceSeries[dataLen] < index[dataLen] && priceSeries[dataLen - 1] > index[dataLen - 1] && account.positionStatus == 1)
                tradeSignal = -1;//死叉且持仓平多
            else
                tradeSignal = 0;

            tradeInfo[0] = tradeSignal;
            if (tradeSignal == 1)
                tradeInfo[1] = priceSeries[dataLen] * (1 + 0.002);//当前价+0.2%的冲击成本
            else if (tradeSignal == -1)
                tradeInfo[1] = priceSeries[dataLen] * (1 - 0.002);//当前价+0.2%的冲击成本
            else
                tradeInfo[1] = priceSeries[dataLen];//无信号时返回实时行情
            
            tradeInfo[2] = 1;//初始以1手为基本交易量

            return tradeInfo;
        }
    }
}
