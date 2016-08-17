using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
    class DateUtilsTests
    {

        public static void test1()
        {
            Console.WriteLine(DateUtils.NextTradeDay(new DateTime(2016,4,3)));
            Console.WriteLine(DateUtils.GetThirdFridayOfMonth(2016,6));
            Console.WriteLine(DateUtils.GetThirdFridayOfMonth(2016, 7));
            Console.WriteLine(DateUtils.GetThirdFridayOfMonth(2016, 8));
        }
    }
}
