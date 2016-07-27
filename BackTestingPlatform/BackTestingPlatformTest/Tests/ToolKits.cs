using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;
using Autofac;
using BackTestingPlatform.DataAccess;
using System.IO;

namespace BackTestingPlatform.Tests
{
    public class ToolKits
    {

        public static void SaveAllTradeDays(int year1,int year2)
        {
            DateTime date1 = new DateTime(year1, 1, 1);
            DateTime date2 = new DateTime(year2+1, 1, 1);
            TradeDaysInfoRepository repo = Platforms.container.Resolve<TradeDaysInfoRepositoryFromWind>();
            var d = repo.fetch(date1,date2);

            Console.WriteLine("fetch {0} trade days from Wind.",d.Count());
            var filePath = String.Format("trade_days_{0}_{1}.txt",year1,year2);
            using (var sw = new StreamWriter(filePath))
            {
                for (int i = 0; i < d.Count; i++)
                {
                    sw.WriteLine(d[i].ToString("yyyyMMdd"));
                }
            }

            Console.WriteLine("file ({0}) saved.", filePath);
        }

        public static void readFile()
        {
            using (StreamReader reader = new StreamReader("x.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    
                    Console.WriteLine(line); // Write to console.
                }
            }
        }
       


    }
}
