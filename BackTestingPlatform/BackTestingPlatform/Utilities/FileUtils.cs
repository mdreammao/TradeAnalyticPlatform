using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    public static class FileUtils
    {
        /// <summary>
        /// 获取该app的根路径，目前没找到比较完美的办法
        /// </summary>
        /// <returns></returns>
        public static string GetAppRootPath()
        {
            if (_appRootPath != null) return _appRootPath;
            //首次调用该方法时会计算_appRootPath
            _appRootPath = System.Environment.CurrentDirectory;
            if (_appRootPath.EndsWith("bin\\Debug")) _appRootPath = _appRootPath.Substring(0, _appRootPath.Length - 10);
            return _appRootPath;
        }
        private static string _appRootPath = null;

        /// <summary>
        /// 根据给定的key和app.config生成CacheData文件路径,包含当前日期后缀
        /// </summary>
        /// <param name="key">例如"CacheData.Path.OptionInfo"</param>
        /// <returns>例如TradeDays_20160803.txt</returns>
        public static string GetCacheDataFilePath(string key, DateTime timestamp)
        {
            return ConfigurationManager.AppSettings["CacheData.RootPath"]
                + ConfigurationManager.AppSettings[key].Replace("{0}", timestamp.ToString("yyyyMMdd"));
        }

        public static string GetCacheDataFilePath(string key)
        {
            return
                ConfigurationManager.AppSettings["CacheData.RootPath"]
               + ConfigurationManager.AppSettings[key];
        }
        /// <summary>
        /// 根据key获取路径配置，列出所有匹配的文件，按文件名倒序排列
        /// </summary>
        /// <param name="key">app.config中的key</param>
        /// <returns></returns>
        public static List<string> GetCacheDataFiles(string key)
        {
            var path = FileUtils.GetCacheDataFilePath(key);
            var dirPath = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);
            return Directory.EnumerateFiles(dirPath, fileName.Replace("{0}", "*"))
                .OrderByDescending(fn => fn).ToList();
        }

        public static string GetCacheDataFileThatLatest(string key)
        {
            var list = GetCacheDataFiles(key);
            return (list != null && list.Count > 0) ? list[0] : null;
        }



        public static string GetCacheDataFileByCodeAndDate(string key,string code,DateTime date)
        {
            var path = FileUtils.GetCacheDataFilePath(key);
            string[] str = code.Split('.');
            string dateStr = date.ToString("yyyyMMdd");
            return path.Replace("{0}", str[0]).Replace("{1}", str[1]).Replace("{2}", dateStr);

        }
        public static DateTime GetCacheDataFileTimestamp(string filePath)
        {
            int x1 = filePath.LastIndexOf('_');
            int x2 = filePath.LastIndexOf('.');
            string timeStr = filePath.Substring(x1 + 1, x2 - x1 - 1);
            return Kit.ToDate(timeStr);
        }


        /// <summary>
        /// 计算出CacheDataFile中的指定文件的时间戳和今天所差的天数，
        /// 返回值=今天-该文件的时间戳，如果没有找到文件则返回36500（100年）
        /// </summary>      
        /// <returns></returns>
        public static int GetCacheDataFileDaysPastTillToday(string filePath)
        {
            if (filePath != null)
            {
                var timestamp = FileUtils.GetCacheDataFileTimestamp(filePath);
                return (DateTime.Now - timestamp).Days;
            }
            else
            {
                return 36500;
            }
        }

    }


}
