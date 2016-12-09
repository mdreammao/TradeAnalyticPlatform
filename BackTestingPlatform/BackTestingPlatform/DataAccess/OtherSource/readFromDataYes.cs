using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.OtherSource
{
    public static class readFromDataYes
    {
        public static void getData(string path,string fileName)
        {
            Dictionary<string, Dictionary<int, List<KLine>>> dic = new Dictionary<string, Dictionary<int, List<KLine>>>();
            string header = "time,code,open,close,high,low,volume,turnover,totalvolume,totalturnover,requesttime";
            DataTable dt = CsvFileUtils.ReadFromCsvFile(path,false,header);
            foreach (DataRow row in dt.Rows)
            {
                var time = row["time"];
                string code =Convert.ToString(row["code"]);
                double open = Convert.ToDouble(row["open"]);
                double close = Convert.ToDouble(row["close"]);
                double high = Convert.ToDouble(row["high"]);
                double low = Convert.ToDouble(row["low"]);
                double volume = Convert.ToDouble(row["volume"]);
                double turnover = Convert.ToDouble(row["turnover"]);
            }

        }
    }
}
