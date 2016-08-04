using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            var fn = ConfigurationManager.AppSettings[key];
            int fnl = fn.Length;
            return
                ConfigurationManager.AppSettings["CacheData.RootPath"]
               + fn.Substring(0,fnl-4) + timestamp.ToString("_yyyyMMdd")+fn.Substring(fnl-4,4);
        }

        public static string GetCacheDataFilePath(string key)
        {
            return
                ConfigurationManager.AppSettings["CacheData.RootPath"]
               + ConfigurationManager.AppSettings[key];
        }
    }


}
