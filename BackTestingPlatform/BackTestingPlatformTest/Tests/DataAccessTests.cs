using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
    public class DataAccessTests
    {
        public static void testOptionDailyRepo()
        {
            var repo = Platforms.container.Resolve<OptionDailyRepository>();
            var rr=repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
        }

        public static void testTickRepo()
        {
            var repo2 = Platforms.container.Resolve<TickRepository>();
            var rrr = repo2.fetchFromLocalCsvOrMssqlAndSave("510050.SH", Kit.ToDate("20160811"));
            Console.WriteLine(rrr.Count);
        }
        public static void test1()
        {
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            //var OptionInfoList = repo.readFromWind();
            var OptionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
        }
        public static void testStockMinuteKLineRepo()
        {
            var repo3 = Platforms.container.Resolve<StockMinuteRepository>();
            var rr = repo3.fetchFromLocalCsvOrWindAndSave("000977.SH", Kit.ToDate(20160810));
        }
    }
}
