using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Monitor.Model;
using BackTestingPlatform.Monitor.Option.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.DataApplication;
using BackTestingPlatform.Utilities.Option;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;


namespace BackTestingPlatform.Monitor.IndexCorr
{
    public class IndexCorr
    {

        
        DateTime today = new DateTime();
        DateTime IHstartDate = new DateTime();
        List<DateTime> tradeDays = new List<DateTime>();
        int period = 250;
        public IndexCorr(int todayInt)
        {
            IHstartDate = new DateTime(2015, 4, 16);
            today = Kit.ToDate(todayInt);
            tradeDays = DateUtils.GetTradeDays(new DateTime(2015,4,13), today.AddDays(-1));
        }

        public void CorrOfIHandIC()
        {

        }
        public void  CorrOf100and500()
        {
            List<StockMinute> all100 = new List<StockMinute>();
            List<StockMinute> all500 = new List<StockMinute>();
            List<CorrStatic> corrList = new List<CorrStatic>();
            for (int i = 0; i < tradeDays.Count(); i++)
            {
                var now = tradeDays[i];
                var index100 = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave("399330.SZ", now);
                var index500 = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave("000905.SZ", now);
                all100.AddRange(index100);
                all500.AddRange(index500);
            }
            for (int i = 0; i < tradeDays.Count()-period; i++)
            {
                DateTime start = tradeDays[i];
                DateTime end = tradeDays[i + period];
                int startIndex = i * 240;
                int endIndex = (i + period) * 240 - 1;
                CorrStatic corr = new CorrStatic() { start = start, end = end, underlying1 = "399330.SZ", underlying2 = "000905.SZ", corr = 0 };
                List<double> underlying1 = new List<double>();
                List<double> underlying2 = new List<double>();
                for (int j = startIndex; j <endIndex ; j++)
                {
                    underlying1.Add(all100[j].close);
                    underlying2.Add(all500[j].close);
                }
                corr.corr = getCorr(underlying1, underlying2);
                corrList.Add(corr);
            }
            var dt = DataTableUtils.ToDataTable(corrList);
            string path = "d:\\corr0.csv";
            try
            {
                var s = (File.Exists(path)) ? "覆盖" : "新增";
                CsvFileUtils.WriteToCsvFile(path, dt);
            }
            catch (Exception e)
            {

            }
        }

        public double getCorr(List<double> list1,List<double> list2)
        {

            double mean1=0, mean2 = 0;
            double moment2nd1 = 0, moment2nd2 = 0;
            double mean12 = 0;
            double length = list1.Count();
            double corr = 0;
            for (int i = 0; i < list1.Count(); i++)
            {
                mean1 += list1[i];
                mean2 += list2[i];
                mean12 += list1[i] * list2[i];
                moment2nd1 += list1[i] * list1[i];
                moment2nd2 += list2[i] * list2[i];
            }
            mean1 /= length;
            mean2 /= length;
            mean12 /= length;
            moment2nd1 /= length;
            moment2nd2 /= length;
            corr = (mean12 - mean1 * mean2) / (Math.Sqrt((moment2nd1 - mean1 * mean1) * (moment2nd2 - mean2 * mean2)));
            return corr;
        }

    }
}
