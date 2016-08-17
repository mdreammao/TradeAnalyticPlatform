using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
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
    public class OptionSample2
    {


        public class positionShot
        {
            public DateTime time { get; set; }
            public double etfPrice { get; set; }

            public SortedDictionary<string, OptionShot> option { get; set; }
            
        }

        public class OptionShot
        {
            public double strike,ask, bid, last, askv, bidv;
        }
       
        public OptionSample2(int start, int end)
        {
            var days = Caches.getTradeDays();
            //OptionInfoService optionInfoService = Platforms.container.Resolve<OptionInfoService>();
            //optionInfoService.loadOptionInfo("510050.SH", "sse");
            //var optionInfo = Caches.get<List<OptionDaily>>("OptionInfos");
            //days = TradeDaysUtils.getTradeDays(start, end);
            //List<positionShot> answer = new List<positionShot>();

            //foreach (var item in (List<DateTime>)days)
            //{
            //    SortedDictionary<string, List<TickFromMssql>> optionList = new SortedDictionary<string, List<TickFromMssql>>();
            //    StockTickDataRepository etfTick = Platforms.container.Resolve<StockTickDataRepository>();
            //    var etf =DataListUtils.FillList(etfTick.fetchDataFromMssql("510050.SH", item));
            //    var optionToday = OptionUtilities.getOptionListByDate((List<OptionInfo>)optionInfo, Kit.ToInt_yyyyMMdd(item));
            //    positionShot now = new positionShot();
                
            //    foreach (var options in optionToday)
            //    {
            //        OptionTickRepository optionTick = Platforms.container.Resolve<OptionTickRepository>();
            //        var option =DataListUtils.FillList(optionTick.fetchDataFromMssql(options.optionCode, item));
            //        optionList.Add(options.optionCode, option);
            //    }
            //    for (int i = 0; i < etf.Count; i++)
            //    {
            //        positionShot shot = new positionShot();
            //        shot.etfPrice = etf[i].lastPrice;
            //        shot.time =Kit.ToDateTime(etf[i].date,etf[i].time);
            //        SortedDictionary<string, OptionShot> option = new SortedDictionary<string, OptionShot>();
            //        foreach (var option0 in optionToday)
            //        {
            //            OptionShot shot0 = new OptionShot();
            //            shot0.strike = option0.strike;
            //            shot0.ask = optionList[option0.optionCode][i].ask[0].price;
            //            shot0.bid= optionList[option0.optionCode][i].bid[0].price;
            //            shot0.askv = optionList[option0.optionCode][i].ask[0].volume;
            //            shot0.bidv = optionList[option0.optionCode][i].bid[0].volume;
            //            shot0.last = optionList[option0.optionCode][i].lastPrice;
            //            option.Add(option0.optionCode, shot0);
            //        }
            //        shot.option = option;
            //        answer.Add(shot);
            //    }
            //}
            //saveToLocalFile(answer, "positionShot.csv");
            //List<string[]> answerList = new List<string[]>();
            //for (int obs = 0; obs < answer.Count; obs++)
            //{
            //    answerList[obs][0] = answer[obs].time.ToString();
            //    answerList[obs][1] = answer[obs].etfPrice.ToString();
            //    //  answerList[obs][2] = answer[obs].option;
            //}
            //bool append = true;
            /*
            StreamWriter fileWriter = new StreamWriter("positionShot.csv", append, Encoding.Default);
            
                        foreach (string[] strArr in answer)
                        {
                            fileWriter.WriteLine(string.Join(",", strArr));
                        }
                        fileWriter.Flush();
                        fileWriter.Close();
             */

        }

        public void saveToLocalFile(List<positionShot> optionMinuteData, string path)
        {
            var dt = DataTableUtils.ToDataTable(optionMinuteData);
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);
        }

        public List<OptionMinuteKLineWithUnderlying> AddEtfPrice(List<OptionMinuteKLine> option, List<KLine> etf, OptionDaily optionInfo)
        {
            if (option.Count != 240 || etf.Count != 240)
            {
                return null;
            }
            List<OptionMinuteKLineWithUnderlying> items = new List<OptionMinuteKLineWithUnderlying>();
            for (int i = 0; i < 240; i++)
            {
                items.Add(new OptionMinuteKLineWithUnderlying
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
