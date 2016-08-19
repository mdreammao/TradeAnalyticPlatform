using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Option;
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
        }

      public void compute()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            var OptionInfoList = repo.readFromWind();
            Caches.put("OptionInfo", OptionInfoList);
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);
            foreach (var day in tradeDays)
            {
                var list = OptionUtilities.getOptionListByDate(OptionInfoList, Kit.ToInt_yyyyMMdd(day));
                foreach (var info in list)
                {
                    string IHCode = OptionUtilities.getCorrespondingIHCode(info, Kit.ToInt_yyyyMMdd(day));
                    if (IHCode!=null)
                    {
                        //Console.WriteLine("date: {0}, IH: {1}", Kit.ToInt_yyyyMMdd(day), IHCode);
                        var repoIH = Platforms.container.Resolve<FuturesMinuteRepository>();
                        var IHtoday = repoIH.fetchFromWind(IHCode, day);
                        var repoOption = Platforms.container.Resolve<OptionMinuteRepository>();
                        var optionToday = repoOption.fetchFromWind(info.optionCode, day);
                    }
                }
                
            }
        }

    }
}
