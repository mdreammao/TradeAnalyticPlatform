using BackTestingPlatform.Core;
using BackTestingPlatform.Tests;
using BackTestingPlatformTest.Tests;
using System;

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

            KitTests.test1();
            //OptionDailyRepositoryTests.test();
            DateUtilsTests.test1();

            DataAccessTests.testStockMinuteKLineRepo();
            Console.ReadKey();
        }
    }
}
