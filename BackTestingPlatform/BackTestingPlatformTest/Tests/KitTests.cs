using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
    class KitTests
    {

        public static void test1()
        {
            Console.WriteLine(Kit.ToDate("2016/9/3"));
            Console.WriteLine(Kit.ToDate("2016/9/3 9:30:55"));
            Console.WriteLine(Kit.ToDate("20160705093055"));
            Console.WriteLine(Kit.ToDate(20160705093055));
            Console.WriteLine(Kit.ToDate(20160903));
            Console.WriteLine(Kit.ToDate("20160903"));
            Console.WriteLine(Kit.ToDate(20160903));
            Console.WriteLine(Kit.ToDateTime("2016/9/3 9:30:55"));
            
            Console.WriteLine(Kit.ToDateTime("20160903093055"));
            Console.WriteLine(Kit.ToDateTime(20160903093055));
        }
    }
}
