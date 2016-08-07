using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Service;
using BackTestingPlatform.Service.Option;
using BackTestingPlatform.Service.Stock;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option
{
    public class OptionSample
    {

        public OptionSample(int start,int end)
        {
            var days = Platforms.BasicInfo["TradeDays"];
            OptionInfoService optionInfoService = Platforms.container.Resolve<OptionInfoService>();
            optionInfoService.loadOptionInfo("510050.SH", "sse");
            var optionInfo = Platforms.BasicInfo["OptionInfos"];
            days = TradeDaysUtils.getTradeDays(start, end);
            foreach (var item in (List<DateTime>)days)
            {
                StockMinuteDataService etfData = Platforms.container.Resolve<StockMinuteDataService>();
                var etfMinuteData = etfData.loadStockMinuteData("510050.SH", item);

                var optionToday = OptionUtilities.getOptionListByDate((List<OptionInfo>)optionInfo, Kit.toDateInt(item));
                foreach (var options in optionToday)
                {
                   // if (Utilities.TradeDaysUtils.GetSpanOfTradeDays(item,options.endDate)<=7)
                    {
                        OptionMinuteDataService optionData = Platforms.container.Resolve<OptionMinuteDataService>();
                        var optionMinuteData = optionData.loadOptionMinuteData(options.optionCode, item);
                    }
                }

            }
        }
    }
}
