using BackTestingPlatform.Core;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess
{
    public class TradeDaysRepository
    {

        /// <summary>
        /// 从万德API获取
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<DateTime> fetchFromWind(DateTime startTime, DateTime endTime)
        {
            WindAPI wapi = Platforms.GetWindAPI();
            WindData wd = wapi.tdays(startTime, endTime, "");
            var wdd = (object[])wd.data;
            return wdd.Select(x => (DateTime)x).ToList();

        }

        /// <summary>
        /// 从本地文件获取
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<DateTime> fetchFromLocalFile(DateTime startTime, DateTime endTime)
        {
            var path = FileUtils.GetCacheDataFilePath("CacheData.Path.TradeDays", DateTime.Now);
            if (!File.Exists(path)) return null;

            string[] lines = File.ReadAllLines(path);
            return lines.Select(
              s => DateTime.ParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture)
            ).ToList();
        }

        public void saveToLocalFile(List<DateTime> tradeDays)
        {
            var path = FileUtils.GetCacheDataFilePath("CacheData.Path.TradeDays", DateTime.Now);
            File.WriteAllLines(path, tradeDays.Select(x => x.ToString("yyyyMMdd")));
        }

    }


}
