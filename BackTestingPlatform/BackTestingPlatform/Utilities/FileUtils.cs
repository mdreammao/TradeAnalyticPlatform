using System;
using System.Collections.Generic;
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
    }
}
