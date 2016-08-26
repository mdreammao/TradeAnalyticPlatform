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
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            var OptionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(1);
            Caches.put("OptionInfo", OptionInfoList);
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);
            var ETFDaily = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", 2015);
            foreach (var day in tradeDays)
            {
                Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
                var list = OptionUtilities.getOptionListByDate(OptionInfoList, Kit.ToInt_yyyyMMdd(day));
                var ETFtoday = Platforms.container.Resolve<StockMinuteRepository>().fetchFromWind("510050.SH", day);
                data.Add("510050.SH", ETFtoday.Cast<KLine>().ToList());
                foreach (var info in list)
                {
                    string IHCode = OptionUtilities.getCorrespondingIHCode(info, Kit.ToInt_yyyyMMdd(day));
                    if (IHCode!=null)
                    {
                        //Console.WriteLine("date: {0}, IH: {1}", Kit.ToInt_yyyyMMdd(day), IHCode);
                        var repoIH = Platforms.container.Resolve<FuturesMinuteRepository>();
                        var IHtoday = repoIH.fetchFromLocalCsvOrWindAndSave(IHCode, day);
                        //var IHtick = Platforms.container.Resolve<FuturesTickRepository>().fetchFromMssql(IHCode, day);
                        if (data.ContainsKey(IHCode)==false)
                        {
                            data.Add(IHCode, IHtoday.Cast<KLine>().ToList());
                        }
                        var repoOption = Platforms.container.Resolve<OptionMinuteRepository>();
                        var optionToday = repoOption.fetchFromLocalCsvOrWindAndSave(info.optionCode, day);
                        //var optiontick = Platforms.container.Resolve<OptionTickRepository>().fetchFromMssql(info.optionCode, day);
                        if (data.ContainsKey(info.optionCode)==false)
                        {
                            data.Add(info.optionCode, optionToday.Cast<KLine>().ToList());
                        }
                    }
                }
                //int index = 0;
                //Dictionary<string, List<BasicPositions>> positions = new Dictionary<string, List<BasicPositions>>();
                //while (index < 240)
                //{
                //    Dictionary<string, BasicSignal> signal = new Dictionary<string, BasicSignal>();
                //    foreach (var item in data)
                //    {

                //    }
                //    DateTime next = RawTransaction.computePositions(signal, data, ref positions);
                //    index = index + 1;
                //}
                //print(positions);
            }
        }

    }
}
