using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Sample
{
    class TradeDaysDirectPrint : Strategy
    {
        KLinesDataRepository repo;
        public void act()
        {
           
            repo= Platforms.container.Resolve<KLinesDataRepository>();
            
            var d = repo.fetch("510050.SH", new DateTime(2015, 6, 26), new DateTime(2016, 7, 26));
            for (int i = 0; i < d.Count; i++)
            {              
                Console.WriteLine("{0,8:F3} {1,8:F3} {2,8:F3} {3,8:F3}", d[i].high,d[i].open,d[i].low, d[i].close);
                if (i % 50 == 49)
                {
                    System.Console.WriteLine("--Press any key--");
                    System.Console.ReadKey();
                }
            }
        }
    }
}
