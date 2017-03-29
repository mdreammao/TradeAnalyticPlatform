using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Utilities.TALibrary;
using BackTestingPlatform.Monitor.Model;
using System.IO;

namespace BackTestingPlatform.Monitor.MDCE
{
    public class MDCE
    {
        private DateTime today;
        private readonly int period1 = 5;
        private readonly int period2 = 10;
        public MDCE(int todayInt=0)
        {
            if (todayInt == 0)
            {
                today = DateTime.Now;
            }
            else
            {
                today = Kit.ToDate(todayInt);
            }
        }

        public void compute()
        {
            List<HistoricalVol> volList = new List<HistoricalVol>();
            List<DateTime> tradeDays = DateUtils.GetTradeDays(today.AddDays(-360), today.AddDays(-1));
            for (int i =0; i < tradeDays.Count(); i++)
            {
                DateTime time = tradeDays[i];
                HistoricalVol vol = new HistoricalVol();
                vol.time = time;
                volList.Add(vol);
            }
            computeVol("M1705.DCE", period1, ref volList);
            computeVol("M1705.DCE", period2, ref volList);
            computeVol("M1707.DCE", period1, ref volList);
            computeVol("M1707.DCE", period2, ref volList);
            computeVol("M1708.DCE", period1, ref volList);
            computeVol("M1708.DCE", period2, ref volList);
            computeVol("M1709.DCE", period1, ref volList);
            computeVol("M1709.DCE", period2, ref volList);
            computeVol("M1711.DCE", period1, ref volList);
            computeVol("M1711.DCE", period2, ref volList);
            computeVol("M1712.DCE", period1, ref volList);
            computeVol("M1712.DCE", period2, ref volList);
            computeVol("M1801.DCE", period1, ref volList);
            computeVol("M1801.DCE", period2, ref volList);
            computeVol("M1803.DCE", period1, ref volList);
            computeVol("M1803.DCE", period2, ref volList);
            var dt = DataTableUtils.ToDataTable(volList);
            string path = "historicalVol.csv";
            try
            {
                var s = (File.Exists(path)) ? "覆盖" : "新增";
                CsvFileUtils.WriteToCsvFile(path, dt);
            }
            catch (Exception e)
            {
                
            }
        }

        private void computeVol(string code,int period,ref List<HistoricalVol> volList)
        {
            var list = getHistoricalDailyData(code);
            var mdata = (from x in list where x.close > 0 select x).ToList();
            var timelist = mdata.Select(x => x.time).ToArray();
            var closelist = mdata.Select(x => x.close).ToArray();
            var vol = Volatility.HVYearly(closelist, period);
            int start = volList.Count() - vol.Count();
            string[] monthList = code.Split('.');
            string month = monthList[0].Substring(monthList[0].Length - 2, 2);
            for (int i = start; i < volList.Count(); i++)
            {
                if (month=="01" && period==period1)  
                {
                    volList[i].M01_1 = vol[i - start];
                }
                else if (month == "01" && period == period2)
                {
                    volList[i].M01_2 = vol[i - start];
                }
                else if (month == "03" && period == period1)
                {
                    volList[i].M03_1 = vol[i - start];
                }
                else if (month == "03" && period == period2)
                {
                    volList[i].M03_2 = vol[i - start];
                }
                else if (month == "05" && period == period1)
                {
                    volList[i].M05_1 = vol[i - start];
                }
                else if (month == "05" && period == period2)
                {
                    volList[i].M05_2 = vol[i - start];
                }
                else if (month == "07" && period == period1)
                {
                    volList[i].M07_1 = vol[i - start];
                }
                else if (month == "07" && period == period2)
                {
                    volList[i].M07_2 = vol[i - start];
                }
                else if (month == "08" && period == period1)
                {
                    volList[i].M08_1 = vol[i - start];
                }
                else if (month == "08" && period == period2)
                {
                    volList[i].M08_2 = vol[i - start];
                }
                else if (month == "09" && period == period1)
                {
                    volList[i].M09_1 = vol[i - start];
                }
                else if (month == "09" && period == period2)
                {
                    volList[i].M09_2 = vol[i - start];
                }
                else if (month == "11" && period == period1)
                {
                    volList[i].M11_1 = vol[i - start];
                }
                else if (month == "11" && period == period2)
                {
                    volList[i].M11_2 = vol[i - start];
                }
                else if (month == "12" && period == period1)
                {
                    volList[i].M12_1 = vol[i - start];
                }
                else if (month == "12" && period == period2)
                {
                    volList[i].M12_2 = vol[i - start];
                }
            }

        }

        private List<FuturesDaily> getHistoricalDailyData(string code)
        {
            return Platforms.container.Resolve<FuturesDailyRepository>().fetchFromLocalCsvOrWindAndSave(code, today.AddDays(-360), today.AddDays(-1));
        }
    }
}
