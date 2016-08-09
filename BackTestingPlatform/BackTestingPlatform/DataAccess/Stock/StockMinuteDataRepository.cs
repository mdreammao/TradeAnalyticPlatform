using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Stock
{
    class StockMinuteDataRepository
    {
        public const string PATH_KEY = "CacheData.Path.StockMinuteData";
        public List<StockMinuteData> fetchMinuteDataFromWind(string stockCode, DateTime time)
        {
            WindAPI w = Platforms.GetWindAPI();
            var timeStr = time.ToString("yyyyMM");
            string start = time.ToString("yyyy-MM-dd 00:00:00");
            string end = time.ToString("yyyy-MM-dd 23:59:59");
            WindData wd = w.wsi(stockCode.ToUpper(), "open,high,low,close,volume,amt", start, end, "periodstart=09:30:00;periodend=15:00:00;Fill=Previous");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<StockMinuteData> items = new List<StockMinuteData>(len * fieldLen);
            double[] dataList = (double[])wd.data;
            DateTime[] timeList = wd.timeList;
            for (int k = 0; k < len; k++)
            {
                items.Add(new StockMinuteData
                {
                    time = timeList[k],
                    open = (double)dataList[k * fieldLen + 0],
                    high = (double)dataList[k * fieldLen + 1],
                    low = (double)dataList[k * fieldLen + 2],
                    close = (double)dataList[k * fieldLen + 3],
                    volume = (double)dataList[k * fieldLen + 4],
                    amount = (double)dataList[k * fieldLen + 5]
                });
            }
            return items;
        }

        public void saveToLocalFile(List<StockMinuteData> optionMinuteData, string path)
        {
            var dt = DataTableUtils.ToDataTable(optionMinuteData);
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);
        }

        public List<StockMinuteData> fetchAllFromLocalFile(string filePath)
        {

            if (!File.Exists(filePath)) return null;
            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            List<StockMinuteData> items = new List<StockMinuteData>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var r = dt.Rows[i];
                
                items.Add(new StockMinuteData {
                 time = Convert.ToDateTime(_toString(r["time"])),
                 open = Convert.ToDouble(_toString(r["open"])),
                 high = Convert.ToDouble(_toString(r["high"])),
                 low = Convert.ToDouble(_toString(r["low"])),
                 close = Convert.ToDouble(_toString(r["close"])),
                 volume = Convert.ToDouble(_toString(r["volume"])),
                 amount = Convert.ToDouble(_toString(r["amount"]))
                });
            }
            return items;
            //return dt.AsEnumerable().Select(row => new StockMinuteData
            //{
            //    time = Convert.ToDateTime(_toString("time")),
            //    open = Convert.ToDouble(_toString("open")),
            //    high = Convert.ToDouble(_toString("high")),
            //    low = Convert.ToDouble(_toString("low")),
            //    close = Convert.ToDouble(_toString("close")),
            //    volume = Convert.ToDouble(_toString("volume")),
            //    amount = Convert.ToDouble(_toString("amount")),
            //}).ToList();
        }

        private string _toString(object item)
        {
            return Convert.ToString(item).Substring(1, Convert.ToString(item).Length - 2);
        }
    }
}

