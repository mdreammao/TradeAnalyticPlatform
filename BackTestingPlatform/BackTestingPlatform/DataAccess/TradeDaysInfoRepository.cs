using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{
    public interface TradeDaysInfoRepository
    {
        List<DateTime> fetch(DateTime startTime, DateTime endTime);

    }

    /// <summary>
    /// 从万德API获取数据的实现
    /// </summary>
    public class TradeDaysInfoRepositoryFromWind : TradeDaysInfoRepository
    {
        public List<DateTime> fetch(DateTime startTime, DateTime endTime)
        {
            WindAPI wapi = Platforms.GetWindAPI();
            Console.WriteLine(wapi.isconnected());
            WindData wd=wapi.tdays(startTime, endTime, "");
            var wdd = (object[])wd.data;
            var res = new List<DateTime>(wdd.Length);
            for (var i = 0; i < wdd.Length; i++)
            {
                res.Add((DateTime)wdd[i]);
            }
            return res;
        }

       
    }
}
