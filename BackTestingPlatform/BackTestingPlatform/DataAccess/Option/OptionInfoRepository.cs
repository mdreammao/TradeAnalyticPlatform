using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;
using System.IO;
using System.Linq;
using BackTestingPlatform.Utilities;
using System.Configuration;

namespace BackTestingPlatform.DataAccess.Option
{

    public class OptionInfoRepository
    {
        public List<OptionInfo> fetchFromWind(string underlyingCode = "510050.SH", string market = "sse")
        {
            WindAPI wapi = Platforms.GetWindAPI();
            WindData wd = wapi.wset("optioncontractbasicinfo", "exchange=" + market + ";windcode=" + underlyingCode + ";status=all");
            int len = wd.codeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<OptionInfo> items = new List<OptionInfo>(len * fieldLen);
            object[] dm = (object[])wd.data;
            string marketStr = "";
            if (market == "sse")
            {
                marketStr = ".SH";
            }
            for (int k = 0; k < len; k++)
            {
                items.Add(new OptionInfo
                {
                    optionCode = (string)dm[k * fieldLen + 0] + marketStr,
                    optionName = (string)dm[k * fieldLen + 1],
                    executeType = (string)dm[k * fieldLen + 5],
                    strike = (double)dm[k * fieldLen + 6],
                    optionType = (string)dm[k * fieldLen + 4],
                    startDate = (DateTime)dm[k * fieldLen + 9],
                    endDate = (DateTime)dm[k * fieldLen + 10]
                });
            }
            return items;
        }

        public List<OptionInfo> fetchAllFromLocalFile(string underlyingCode = "510050.SH", string market = "sse")
        {

            var path = Path.Combine(
                ConfigurationManager.AppSettings["CacheData.RootPath"]
               , ConfigurationManager.AppSettings["CacheData.Path.OptionInfo"]);

            if (!File.Exists(path)) return null;

            string[] lines = File.ReadAllLines(path);
            return lines.Select(
              s => new OptionInfo
              {
                  //todo s转换成OptionInfo
              }
            ).ToList();
        }

        public void saveToLocalFile(List<OptionInfo> optionInfoList)
        {
            var path = FileUtils.GetCacheDataFilePath("CacheData.Path.OptionInfo", DateTime.Now);
            File.WriteAllLines(path, optionInfoList.Select(
                    x => String.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\"",
                    new string[] {
                        x.optionCode,
                        x.optionName,
                        x.optionType,
                        x.startDate.ToShortDateString()
                    })
                ).ToArray()
            );
        }
    }


}
