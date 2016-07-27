using BackTestingPlatform.Core;
using BackTestingPlatform.Tests;
using System;

namespace BackTestingPlatform.Test
{
    class TestBooter
    {
        static void Main(string[] args)
        {
            Platforms.Initialize();
            //WindApiTest.testTDay()

            //ToolKits.SaveAllTradeDays(2016, 2016);
            ToolKits.readFile();
            Console.ReadKey();
        }
    }
}
