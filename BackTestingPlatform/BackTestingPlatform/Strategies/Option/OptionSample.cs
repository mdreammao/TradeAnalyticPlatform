using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Service;
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
            OptionService optionService = Platforms.container.Resolve<OptionService>();
            optionService.loadOptionInfo("510050.SH", "sse");
            var option = Platforms.BasicInfo["OptionInfos"];
            days = TradeDaysUtils.getTradeDays(start, end);
            foreach (var item in (List<DateTime>)days)
            {
                
                var optionToday = OptionUtilities.getOptionListByDate((List<OptionInfo>)option, Kit.toDateInt(item));
            }
        }
    }
}
