using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
    public class SeqUtilsTests
    {
        public static void test1()
        {
            var repo=Platforms.container.Resolve<OptionTickRepository>();
            var d=repo.fetchFromLocalCsvOrMssqlAndSave("600958.SH",new DateTime(2016,7,7)).OrderBy(tick=>tick.time).ToList();
            var tl = new TimeLine(new int[] {
                34200000,
                34200500,
                34200700,
                34201100,
                34201600,
                34202000,
                34202600,
                34222000,
                34222100,
                34222200,
                34222300
            });

            var tl2 = new TimeLine(new TimeLineSection("09:30:00.000", "09:40:00.000", 2000));
            
            var r=SequentialUtils.Resample<OptionTickFromMssql>(d, tl2);
        }

        public static void test2()
        {
            var repo = Platforms.container.Resolve<OptionTickRepository>();
            var timeline = Constants.timeline500ms;
            var r=repo.fetchFromLocalCsvOrMssqlAndResampleAndSave("600958.SH", new DateTime(2016, 7, 6), timeline);

        }
    }
}
