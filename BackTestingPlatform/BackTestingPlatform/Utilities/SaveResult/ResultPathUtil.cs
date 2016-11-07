using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.SaveResult
{
    public  class ResultPathUtil
    {
        public static string GetLocalPath(string fullPath,string tag,string dateStr,string type,string parameters,string performance)
        {
            return fullPath.Replace("{tag}", tag).Replace("{date}", dateStr).Replace("{parameters}",parameters).Replace("{type}", type).Replace("{performance}",performance);
        }
    }
}
