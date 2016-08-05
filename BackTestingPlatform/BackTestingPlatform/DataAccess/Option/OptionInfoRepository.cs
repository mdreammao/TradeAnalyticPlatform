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
using System.Data;

namespace BackTestingPlatform.DataAccess.Option
{

    public class OptionInfoRepository
    {
        public const string PATH_KEY = "CacheData.Path.OptionInfo";
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
        
        public List<OptionInfo> fetchAllFromLocalFile(string filePath,string underlyingCode = "510050.SH", string market = "sse")
        {
            if (!File.Exists(filePath)) return null;
            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            //return DataTableUtils.ToList<OptionInfo>(dt); //generic transform
            return dt.AsEnumerable().Select(row => new OptionInfo
            {
                optionCode= (string)row["optionCode"],
                optionName= (string)row["optionName"],
                optionType= (string)row["optionType"],
                startDate=Kit.toDateTime((string)row["startDate"])
            }).ToList();
        }

        

        public void saveToLocalFile(List<OptionInfo> optionInfoList)
        {
            var path = FileUtils.GetCacheDataFilePath(PATH_KEY, DateTime.Now);
            var dt=DataTableUtils.ToDataTable(optionInfoList);
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);

        }
       
    }


}
