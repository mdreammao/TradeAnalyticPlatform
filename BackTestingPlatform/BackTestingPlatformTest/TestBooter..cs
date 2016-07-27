using BackTestingPlatform.Core;
using BackTestingPlatform.Tests;

namespace BackTestingPlatform.Test
{
    class TestBooter
    {
        static void Main(string[] args)
        {
            Platforms.Initialize();
            WindApiTest.testTDay();
        }
    }
}
