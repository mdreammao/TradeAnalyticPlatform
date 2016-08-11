using System;
using BackTestingPlatform.Core;
using BackTestingPlatform.Model.Stock;
using System.Collections.Generic;
using WAPIWrapperCSharp;
using System.Data;

namespace BackTestingPlatform.DataAccess
{
  
    /// <summary>
    /// 从万德API获取A股股票信息。
    /// </summary>
    public class ASharesInfoRepository
    {
        public const string PATH_KEY = "CacheData.Path.ASharesInfoData";
        public List<ASharesInfo> fetchFromWind(DateTime startTime)
        {
            WindAPI wapi = Platforms.GetWindAPI();
            Console.WriteLine(wapi.isconnected());
            string date = startTime.ToString("yyyyMMdd");
            WindData wd = wapi.wset("sectorconstituent", "date = " + date +
                ";sectorid=a001010100000000;field=date,wind_code,sec_name");

            int codeLen = wd.codeList.Length;
            int fieldLen = wd.fieldList.Length;
            object[] dataList = (object[])wd.data;
            
            List<ASharesInfo> items = new List<ASharesInfo>();            
            for (int k = 0; k < codeLen; k += fieldLen)
            {
                items.Add(new ASharesInfo
                {
                    lastTradeDay = (DateTime)dataList[k],
                    stockCode = (string)dataList[k + 1],
                    stockName = (string)dataList[k + 2]
                });
            }
            return items;
        }

        public List<ASharesInfo> fetchFromDatabase(DateTime startTime, string connStr = "corp170")
        {
          
            return null;
        }
    }
}
