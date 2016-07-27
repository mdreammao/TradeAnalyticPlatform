using BackTestingPlatform.DataAccess;
using System;

namespace BackTestingPlatformTest.Tests
{
    class MyPlatformTest
    {
        public static void test()
        {
          
            WsiDataRepository repo = new WsiDataRepositoryFromWind();

            var ddd =
             repo.fetch("510050.SH", new DateTime(2015, 7, 26, 9, 0, 0), new DateTime(2016, 7, 26, 15, 0, 0));
            Console.ReadKey();
        }
    }
}
