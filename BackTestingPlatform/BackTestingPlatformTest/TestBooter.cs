using BackTestingPlatform.Core;
using BackTestingPlatform.Tests;
using BackTestingPlatformTest.Tests;
using System;
using System.Collections.Generic;
using BackTestingPlatform.Charts;
using System.Windows.Forms;

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
            //SeqUtilsTests.test2();
            //DataAccessTests.testStockDailyRepo();
            //DataAccessTests.testStockMinuteKLineRepo();
            //int n = 130000;double x1=0;
            //var xa = new DateTime();
            //int ss = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DateTime));
            //var s = new List<DateTime>(n);
            //for (int i = 0; i < n; i++)
            //{
            //    s.Add(new DateTime(i));
            //    //for (int j = 0; j < 44444; j++) x1 += Math.Sin(i);
            //           // Console.Write(i);
            //}

            
            //测试PLChart
            Dictionary<string, double[]> line = new Dictionary<string, double[]>();
            double[] y1 = { 120, 125, 100, 40, 105, 75 };
            double[] y2 = { 100, 66, 77, 40, 198, 20 };
            line.Add("text1", y1);
            line.Add("text2", y2);
            string[] date = { "2012", "2013" };
            //最好将数据初始化以后，传入图形化类，但是需要考虑传递大数量级形参会不会出什么问题，考虑是传递参数还是数据
            //最后一个数据5表示k线为daily
            Application.Run(new PLChart(line, date));
            

            /*
            //测试图形化
            int startTime = 20160108;
            int endTime = 20160810;
            string secCode = "510050.SH";
            Application.Run(new CandleStick(startTime, endTime, secCode, 5));
            */

            Platforms.ShutDown();
            Console.ReadKey();
        }
    }
}
