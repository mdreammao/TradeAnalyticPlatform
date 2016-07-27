using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BackTestingPlatform.Strategies.Sample
{
    class TradeDaysDirectPrint : Strategy
    {
        KLinesDataRepository repo;
        public void act()
        {
            KLinesDataRepository repo = Platforms.container.Resolve<KLinesDataRepository>();
            
            //计算运行时间
            System.Diagnostics.Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); //  开始监视代码运行时间

            var StockData = repo.fetch("510050.SH", new DateTime(2015, 6, 26), new DateTime(2016, 7, 26));

            stopwatch.Stop(); //  停止监视
            TimeSpan timespan = stopwatch.Elapsed; //  获取当前实例测量得出的总时间
            double seconds = timespan.TotalSeconds;  //  总秒数
            double milliseconds = timespan.TotalMilliseconds;  //  总毫秒数
            System.Console.WriteLine("Running Time1:{0} seconds.", seconds);

            System.Diagnostics.Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start(); //  开始监视代码运行时间

            var StockData2= repo.fetch("510050.SH", new DateTime(2015, 6, 26), new DateTime(2016, 7, 26));

            stopwatch1.Stop(); //  停止监视
            TimeSpan timespan1 = stopwatch1.Elapsed; //  获取当前实例测量得出的总时间
            double seconds1 = timespan1.TotalSeconds;  //  总秒数
            double milliseconds1 = timespan1.TotalMilliseconds;  //  总毫秒数
            System.Console.WriteLine("Running Time2:{0} seconds.", seconds1);
            System.Console.ReadKey();

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
        }
    }
}
