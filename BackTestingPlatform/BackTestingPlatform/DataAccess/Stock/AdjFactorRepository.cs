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
    /// <summary>
    /// 检查本地调整因子是否需要更新，若要更新，从Wind上抓取，并更新本地；若无需更新，从本地抓取
    /// </summary>
    public class AdjFactorRepository
    {
        public const string PATH_KEY = "CacheData.Path.AdjFactorData";
        public List<AdjFactor> fetchFromWind(string stockCode, DateTime startTime,DateTime endTime)
        {
            WindAPI w = Platforms.GetWindAPI();
            string startStr = startTime.ToString("yyyy-MM-dd");
            string endStr = endTime.ToString("yyyy-MM-dd");
            WindData wd = w.wsd(stockCode, "adjfactor", startStr, endStr, "");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<AdjFactor> items = new List<AdjFactor>(len * fieldLen);
            double[] dataList = (double[])wd.data;
            DateTime[] timeList = wd.timeList;
            for (int k = 0; k < len; k++)
            {
                items.Add(new AdjFactor
                {
                    time = timeList[k],
                    code = Int32.Parse(stockCode.Substring(0,stockCode.Length-3)),//取代码字符串转化成整型
                    backfwdFactor = (double)dataList[k * fieldLen + 0],
                });
            }
            return items;
        }

        /// <summary>
        /// 覆盖式写入，之后考虑add形式
        /// </summary>
        /// <param name="adjFactorData"></param>
        /// <param name="path"></param>
        public void saveToLocalFile(List<AdjFactor> adjFactorData, string path)
        {         
            var dt = DataTableUtils.ToDataTable(adjFactorData);
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);
        }

        public List<AdjFactor> fetchAllFromLocalFile(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            List<AdjFactor> items = new List<AdjFactor>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var r = dt.Rows[i];
                items.Add(new AdjFactor
                {
                    time = Convert.ToDateTime(_toString(r["time"])),
                    code = Convert.ToInt32(_toString(r["code"])),
                    backfwdFactor = Convert.ToDouble(_toString(r["backfwdFactor"])),
                });
            }
            return items;
            //return dt.AsEnumerable().Select(row => new OptionMinuteData
            //{
            //    time=Convert.ToDateTime(_toString("time")),
            //    open=Convert.ToDouble(_toString("open")),
            //    high = Convert.ToDouble(_toString("high")),
            //    low  = Convert.ToDouble(_toString("low")),
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
