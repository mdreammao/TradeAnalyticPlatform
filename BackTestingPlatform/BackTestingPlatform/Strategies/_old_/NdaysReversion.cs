using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Transaction;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.TimeList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BackTestingPlatform.Strategies.Stock
{
    class NdaysReversion
    {
        private DateTime startdate, endDate;
        public NdaysReversion(int start, int end)
        {
            startdate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
        }

        public void stg()
        {
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            var OptionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
            Caches.put("OptionInfo", OptionInfoList);
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);
            var ETFDaily = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", Kit.ToDate(20150101), Kit.ToDate(20160731));
            foreach (var day in tradeDays)
            {
                Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
                var list = OptionUtilities.getOptionListByDate(OptionInfoList, Kit.ToInt_yyyyMMdd(day));
                List<DateTime> durationArr = OptionUtilities.getEndDateListByAscending(list);
                var ETFtoday = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", day);
                data.Add("510050.SH", ETFtoday.Cast<KLine>().ToList());
                foreach (var info in list)
                {
                    string IHCode = OptionUtilities.getCorrespondingIHCode(info, Kit.ToInt_yyyyMMdd(day));
                    var repoOption = Platforms.container.Resolve<OptionMinuteRepository>();
                    var optionToday = repoOption.fetchFromLocalCsvOrWindAndSave(info.optionCode, day);
                    data.Add(info.optionCode, optionToday.Cast<KLine>().ToList());
                }
                int index = 0;
                Dictionary<string, List<MinutePositions>> positions = new Dictionary<string, List<MinutePositions>>();
                while (index < 240)
                {
                    Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
                    double etfPrice = ETFtoday[index].close;
                    List<double> strikeTodayArr = OptionUtilities.getStrikeListByAscending(list).OrderBy(x => Math.Abs(x - etfPrice)).ToList();
                    OptionInfo callCandidate = OptionUtilities.getSpecifiedOption(list, durationArr[0], "认购", strikeTodayArr[0])[0];
                    OptionInfo putCandidate = OptionUtilities.getSpecifiedOption(list, durationArr[0], "认沽", strikeTodayArr[0])[0];
                    foreach (var item in data)
                    {
                            
                    }
                    //DateTime next = RawTransaction.computePositions(signal, data, ref positions);
                    index = index + 1;
                }
                //print(positions);
            }
            }
        }
}
