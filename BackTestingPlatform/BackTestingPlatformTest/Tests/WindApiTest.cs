using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.Tests
{
    class WindApiTest
    {

        void test1()
        {
            WindAPI w = new WindAPI();
            w.start();
            WindData wd = w.wsi("510050.SH", "open,high,low,close", "2016-07-26 09:00:00", "2016-07-26 14:56:12", "");

        }

        static void Main(string[] args)
        {
            WindApiTest t = new WindApiTest();
            t.test1();
        }


    }
}
