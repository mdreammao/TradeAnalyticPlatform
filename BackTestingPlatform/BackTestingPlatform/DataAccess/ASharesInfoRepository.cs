using System;
using BackTestingPlatform.Core;
using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{
    public interface ASharesInfoRepository
    {
        List<ASharesInfo> fetch();
    }

    /// <summary>
    /// 从万德API获取A股股票信息。
    /// </summary>
    public class ASharesInfoRepositoryFromWind : ASharesInfoRepository
    {
        List<ASharesInfo> ASharesInfoRepository.fetch()
        {
            WindAPI wapi = Platforms.GetWindAPI();
            Console.WriteLine(wapi.isconnected());
            WindData wd = wapi.wset("listedsecuritygeneralview", "sectorid=a001010100000000");
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;
            List<ASharesInfo> items = new List<ASharesInfo>(len);


        }
    }
}
