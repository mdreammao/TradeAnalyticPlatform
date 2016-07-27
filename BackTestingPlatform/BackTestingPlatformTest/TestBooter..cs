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
            WindApiTest.testKLine();

            Console.ReadKey();
        }
    }
}
