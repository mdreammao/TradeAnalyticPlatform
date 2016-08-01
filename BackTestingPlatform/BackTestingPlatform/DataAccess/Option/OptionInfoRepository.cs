using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;

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
            return items;
        }
    }
}
