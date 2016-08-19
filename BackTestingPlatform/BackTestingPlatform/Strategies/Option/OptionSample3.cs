using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
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
            var OptionInfoList = repo.readFromWind();
            Caches.put("OptionInfo", OptionInfoList);
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startdate, endDate);
            foreach (var day in tradeDays)
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                var list = OptionUtilities.getOptionListByDate(OptionInfoList, Kit.ToInt_yyyyMMdd(day));
                //var ETFtoday = Platforms.container.Resolve<StockMinuteRepository>().fetchFromWind("510050.SH", day);
                //var ETFtick = Platforms.container.Resolve<StockTickRepository>().fetchFromMssql("510050.SH", day);
                //data.Add("510050.SH", ETFtoday);
                foreach (var info in list)
                {
                    string IHCode = OptionUtilities.getCorrespondingIHCode(info, Kit.ToInt_yyyyMMdd(day));
                    if (IHCode!=null)
                    {
                        //Console.WriteLine("date: {0}, IH: {1}", Kit.ToInt_yyyyMMdd(day), IHCode);
                        var repoIH = Platforms.container.Resolve<FuturesMinuteRepository>();
                        //var IHtoday = repoIH.fetchFromWind(IHCode, day);
                        var IHtick = Platforms.container.Resolve<FuturesTickRepository>().fetchFromMssql(IHCode, day);
                        //if (data.ContainsKey(IHCode)==false)
                        //{
                        //    data.Add(IHCode, IHtoday);
                        //}
                        var repoOption = Platforms.container.Resolve<OptionMinuteRepository>();
                       // var optionToday = repoOption.fetchFromWind(info.optionCode, day);
                        var optiontick = Platforms.container.Resolve<OptionTickRepository>().fetchFromMssql(info.optionCode, day);
                        //if (data.ContainsKey(info.optionCode)==false)
                        //{
                        //    data.Add(info.optionCode, optionToday);
                        //}
                    }
                }
                int index = 0;
                Dictionary<string, List<BasicPositions>> positions = new Dictionary<string, List<BasicPositions>>();
                while (index<240)
                {
                    Dictionary<string, BasicSignal> signal = new Dictionary<string, BasicSignal>();
                    foreach (var item in data)
                    {
                       
                    }
                    DateTime next = RawTransaction.computePositions(signal, data,ref positions);
                    index = index + 1;
                }
                //print(positions);
            }
        }

    }
}
