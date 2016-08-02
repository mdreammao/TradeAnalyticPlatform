using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;
using System.IO;
using System.Linq;

namespace BackTestingPlatform.DataAccess.Option
{
    public interface OptionInfoRepository
    {
        List<OptionInfo> fetchAll(string underlyingCode="510050.SH",string market="sse");
    }
    class OptionInfoRepositoryFromWind : OptionInfoRepository
    {
        public List<OptionInfo> fetchAll(string underlyingCode = "510050.SH",string market="sse")
        {
            WindAPI wapi = Platforms.GetWindAPI();
            WindData wd = wapi.wset("optioncontractbasicinfo", "exchange="+market+";windcode="+underlyingCode+";status=all");
            int len = wd.codeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<OptionInfo> items = new List<OptionInfo>(len*fieldLen);
            object[] dm = (object[])wd.data;
            string marketStr="";
            if (market=="sse")
            {
                marketStr = ".SH";
            }
            for (int k = 0; k < len; k++)
            {
                items.Add(new OptionInfo
                {
                    optionCode = (string)dm[k * fieldLen + 0]+marketStr,
                    optionName=(string)dm[k*fieldLen+1],
                    executeType=(string)dm[k*fieldLen+5],
                    strike=(double)dm[k * fieldLen + 6],
                    optionType=(string)dm[k * fieldLen + 4],
                    startDate=(DateTime)dm[k*fieldLen+9],
                    endDate=(DateTime)dm[k*fieldLen+10]
                });
            }
            if (Platforms.basicInfo.ContainsKey("optionInfo"))
            {
                Platforms.basicInfo["optionInfo"] = items;
            }
            else
            {
                Platforms.basicInfo.Add("optionInfo", items);
            }
            return items;
        }
    }

    class OptionInfoRepositoryFromLocalFile : OptionInfoRepository
    {
        public List<OptionInfo> fetchAll(string underlyingCode = "510050.SH", string market = "sse")
        {
            var path = System.Environment.CurrentDirectory;
            if (path.EndsWith("bin\\Debug")) path = path.Substring(0, path.Length - 10);
            path += @"\RESOURCES\optionInfo.csv";
            List<OptionInfo> items = new List<OptionInfo>();
            try
            {
                //希望能简单调用utility的函数来读写
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        
                    }

                    Platforms.basicInfo.Add("optionInfo", items);
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
            return items;
        }
        
    }
    

}
