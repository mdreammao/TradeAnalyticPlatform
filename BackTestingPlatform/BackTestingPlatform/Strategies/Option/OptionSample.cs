using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Common;
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

        public OptionSample(int start, int end)
        {
            var days = Caches.getTradeDays();
            OptionInfoService optionInfoService = Platforms.container.Resolve<OptionInfoService>();
            optionInfoService.loadOptionInfo("510050.SH", "sse");
            var optionInfo = Caches.get<List<OptionInfo>>("OptionInfos");
            days = TradeDaysUtils.getTradeDays(start, end);
            List<OptionMinuteDataWithUnderlying> answer = new List<OptionMinuteDataWithUnderlying>();
            foreach (var item in (List<DateTime>)days)
            {
                StockMinuteDataService etfData = Platforms.container.Resolve<StockMinuteDataService>();
                var etfMinuteData = etfData.loadStockMinuteData("510050.SH", item);
                var optionToday = OptionUtilities.getOptionListByDate((List<OptionInfo>)optionInfo, Kit.ToInt_yyyyMMdd(item));
                foreach (var options in optionToday)
                {
                    #region
                    if (Utilities.TradeDaysUtils.GetSpanOfTradeDays(item, options.endDate) <= 7 && options.optionType == "认购")
                    {
                        OptionMinuteDataService optionData = Platforms.container.Resolve<OptionMinuteDataService>();
                        var optionMinuteData = optionData.loadOptionMinuteData(options.optionCode, item);
                        var optionWithEtf = AddEtfPrice(optionMinuteData, etfMinuteData, options);
                        for (int i = 0; i < 240; i++)
                        {
                            if (options.strike + optionWithEtf[i].close < optionWithEtf[i].underlyingPrice - 0.05 * optionWithEtf[i].close && optionWithEtf[i].volume > 5)
                            {
                                answer.Add(optionWithEtf[i]);
                            }
                        }
                    }
                    #endregion
                }
            }
            saveToLocalFile(answer, "answer.csv");
        }

        public void saveToLocalFile(List<OptionMinuteDataWithUnderlying> optionMinuteData, string path)
        {
            var dt = DataTableUtils.ToDataTable(optionMinuteData);
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);
        }

        public List<OptionMinuteDataWithUnderlying> AddEtfPrice(List<OptionMinuteData> option, List<KLine> etf, OptionInfo optionInfo)
        {
            if (option.Count != 240 || etf.Count != 240)
            {
                return null;
            }
            List<OptionMinuteDataWithUnderlying> items = new List<OptionMinuteDataWithUnderlying>();
            for (int i = 0; i < 240; i++)
            {
                items.Add(new OptionMinuteDataWithUnderlying
                {
                    optionCode = optionInfo.optionCode,
                    optionName = optionInfo.optionName,
                    executeType = optionInfo.executeType,
                    startDate = optionInfo.startDate,
                    endDate = optionInfo.endDate,
                    optionType = optionInfo.optionType,
                    strike = optionInfo.strike,
                    time = option[i].time,
                    open = option[i].open,
                    high = option[i].high,
                    low = option[i].low,
                    close = option[i].close,
                    volume = option[i].volume,
                    amount = option[i].amount,
                    underlyingPrice = etf[i].close
                });
            } 
            return items;
        }
    }
}
