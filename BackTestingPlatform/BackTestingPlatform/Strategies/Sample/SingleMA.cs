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

namespace BackTestingPlatform.Strategies.SingleMA
{
    class SingleMA : Strategy
    {
        KLinesDataRepository repo;
        public void act()
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
            var StockData = repo.fetch("510050.SH", new DateTime(2015, 6, 26), new DateTime(2016, 7, 26));
            double[] priceSeries = new double[StockData.Count];
            DateTime[] dateList = new DateTime[StockData.Count];
            double[] index = new double[StockData.Count];
            int MAParam = 5;

            //取出收盘价
            for (int i = 0; i < StockData.Count; i++)
            {
                priceSeries[i] = StockData[i].close;
                dateList[i] = StockData[i].time;
            }
            TA_MA myMA = new TA_MA(priceSeries);
            index = myMA.SMA(MAParam);

            for (int j = 0; j < index.Length; j++)
            {
                Console.WriteLine("Time:{0,-20} -- MA{1}:{2,8:F3}", dateList[j], MAParam, index[j]);
                if (j % 50 == 49)
                {
                    Console.WriteLine("--Press any key--");
                    Console.ReadKey();
                } 
            }
        }
    }
}
