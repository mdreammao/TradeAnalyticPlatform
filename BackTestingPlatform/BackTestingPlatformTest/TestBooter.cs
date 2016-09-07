using BackTestingPlatform.Core;
using BackTestingPlatform.Tests;
using BackTestingPlatformTest.Tests;
using System;
using System.Collections.Generic;

namespace BackTestingPlatform.Test
{
    class TestBooter
    {
        static void Main(string[] args)
        {
            Platforms.Initialize();
            //WindApiTest.testTDay()

            //ToolKits.SaveAllTradeDays(2010, 2016);
            //ToolKits.readFile();
            //LinqSqlTest.testDataSet();

            //KitTests.test1();
            //OptionDailyRepositoryTests.test();
            //DateUtilsTests.test1();
            //DataAccessTests.testOptionDailyRepo();
            //
            //GenericTypeTests.test1();
            //DataAccessTests.testStockDailyRepo();
            //DataAccessTests.testStockMinuteKLineRepo();
            int n = 130000;double x1=0;
            var xa = new DateTime();
            int ss = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DateTime));
            var s = new List<DateTime>(n);
            for (int i = 0; i < n; i++)
            {
                s.Add(new DateTime(i));
                //for (int j = 0; j < 44444; j++) x1 += Math.Sin(i);
                       // Console.Write(i);
            }
            Platforms.ShutDown();
            Console.ReadKey();
        }
    }
}
