using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
    public class OptionDailyRepositoryTests
    {
        public static void test()
        {
            OptionDailyRepository repo = Platforms.container.Resolve<OptionDailyRepository>();
            repo.fetchFromLocalCsvOrWindAndUpdateAndCache(1);
        }
    }
}
