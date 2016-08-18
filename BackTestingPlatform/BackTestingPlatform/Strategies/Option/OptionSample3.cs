using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option
{
    public class OptionSample3
    {
        private DateTime startdate,endDate;
        public OptionSample3(int start,int end)
        {
            startdate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
            var repo = Platforms.container.Resolve<OptionDailyRepository>();
            var list=OptionUtilities.getOptionListByDate(repo.fetchFromLocalCsvOrWindAndSaveAndCache(1),20150209);
            List<DateTime> tradeDays=DateUtils.GetTradeDays(startdate, endDate);
            foreach (var day in tradeDays)
            {

            }

      }

    }
}
