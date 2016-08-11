using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model;
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
    public class OptionSample2
    {


        public class positionShot
        {
            public DateTime time { get; set; }
            public double etfPrice { get; set; } 
            
        }

       
        public OptionSample2(int start, int end)
        {
            List<int> a = new List<int>();
            var days = Caches.getTradeDays();
            OptionInfoService optionInfoService = Platforms.container.Resolve<OptionInfoService>();
            optionInfoService.loadOptionInfo("510050.SH", "sse");
            var optionInfo = Caches.get<List<OptionInfo>>("OptionInfos");
            days = TradeDaysUtils.getTradeDays(start, end);
            List<positionShot> answer = new List<positionShot>();
            foreach (var item in (List<DateTime>)days)
            {
                StockTickDataRepository etfTick = Platforms.container.Resolve<StockTickDataRepository>();
                var etf = etfTick.fetchDataFromMssql("510050.SH", item);
                var optionToday = OptionUtilities.getOptionListByDate((List<OptionInfo>)optionInfo, Kit.ToInt_yyyyMMdd(item));
                positionShot now = new positionShot();
                
                foreach (var options in optionToday)
                {
                    OptionTickDataRepository optionTick = Platforms.container.Resolve<OptionTickDataRepository>();
                    var option = optionTick.fetchDataFromMssql(options.optionCode, item);

                }
            }
            saveToLocalFile(answer, "positionShot.csv");
        }

        public void saveToLocalFile(List<positionShot> optionMinuteData, string path)
        {
            var dt = DataTableUtils.ToDataTable(optionMinuteData);
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);
        }

        public List<OptionMinuteDataWithUnderlying> AddEtfPrice(List<OptionMinuteData> option, List<StockMinuteData> etf, OptionInfo optionInfo)
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
