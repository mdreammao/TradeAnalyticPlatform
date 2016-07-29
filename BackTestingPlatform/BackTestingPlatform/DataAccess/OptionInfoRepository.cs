using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{
    public interface OptionInfoRepository
    {
        List<OptionInfo> fetchAll(string underlyingCode="510050.SH");
    }
    class OptionInfoRepositoryFromWind : OptionInfoRepository
    {
        public List<OptionInfo> fetchAll(string underlyingCode = "510050.SH")
        {
            WindAPI wapi = Platforms.GetWindAPI();
            WindData wd = wapi.wset("optioncontractbasicinfo", "exchange=sse;windcode="+underlyingCode+";status=all");


        }
    }
}
