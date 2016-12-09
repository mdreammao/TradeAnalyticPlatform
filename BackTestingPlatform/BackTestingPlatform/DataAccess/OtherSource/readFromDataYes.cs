using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.OtherSource
{
    public static class readFromDataYes
    {
        static Logger log = LogManager.GetCurrentClassLogger();
        public static void getData(string path,string fileName)
        {
            Dictionary<string, Dictionary<string, List<KLine>>> dic = new Dictionary<string, Dictionary<string, List<KLine>>>();
            //string header = "time,code,open,close,high,low,volume,turnover,totalvolume,totalturnover,requesttime";
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            string[] row = sr.ReadLine().Split(',').Select(toNonDoubleQuotedString).ToArray();
                            DateTime time =DateTime.ParseExact(Convert.ToString(row[0]),"yyyyMMddHHmm",null);
                            string today = time.ToString("yyyyMMdd");
                            string code = Convert.ToString(row[1]).ToUpper();
                            double open = Convert.ToDouble(row[2])/10000.0;
                            double close = Convert.ToDouble(row[3])/10000.0;
                            double high = Convert.ToDouble(row[4])/10000.0;
                            double low = Convert.ToDouble(row[5])/10000.0;
                            double volume = Convert.ToDouble(row[6]);
                            double turnover = Convert.ToDouble(row[7]);
                            List<KLine> data0 = new List<KLine>();
                            if (dic.ContainsKey(code)==true)
                            {
                                if (dic[code].ContainsKey(today)==true)
                                {
                                    data0 = dic[code][today];
                                }
                                else
                                {
                                    dic[code].Add(today, new List<KLine>());
                                }
                            }
                            else
                            {
                                dic.Add(code, new Dictionary<string, List<KLine>>());
                                dic[code].Add(today, new List<KLine>());
                            }
                            data0.Add(new KLine { time = time, open = open, close = close, high = high, low = low, volume = volume, amount = turnover });
                            dic[code][today] = data0;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }
        static string toDoubleQuotedString(string src)
        {
            return string.Concat("\"", src.Replace("\"", "\"\""), "\"");
        }
        static string toNonDoubleQuotedString(string src)
        {
            int len = src.Length;
            if (src[0] == '\"' && src[len - 1] == '\"')
                return src.Substring(1, len - 2);
            else
                return src;
        }
    }
}
