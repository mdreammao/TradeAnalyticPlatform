using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;
using Autofac;
using BackTestingPlatform.DataAccess;

namespace BackTestingPlatform.Tests
{
    public class WindApiTest
    {

        public static void test1()
        {
            WindAPI w = new WindAPI();
            w.start();
            WindData wd = w.wsi("510050.SH", "open,high,low,close", "2016-07-26 09:00:00", "2016-07-26 14:56:12", "");

        }

        //public static void testTDay()
        //{
        //    TradeDaysInfoRepository repo = Platforms.container.Resolve<TradeDaysInfoRepositoryFromWind>();
        //    var d = repo.fetch(new DateTime(2016, 7, 26, 0, 0, 0), new DateTime(2016, 7, 27, 18, 0, 0));
        //    Console.WriteLine(d.Count());
        //}
        //public static void testKLine()
        //{
        //    KLinesDataRepository repo = Platforms.container.Resolve<KLinesDataRepository>();
        //    var d =
        //     repo.fetch("510050.SH", new DateTime(2015, 7, 26, 9, 0, 0), new DateTime(2016, 7, 26, 15, 0, 0));
        //    Console.WriteLine(d.Count());
        //}



    }
}
