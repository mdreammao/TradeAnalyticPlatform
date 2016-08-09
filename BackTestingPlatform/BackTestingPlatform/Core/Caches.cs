using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Core
{
    /// <summary>
    /// 作用于全局的变量缓存池
    /// </summary>
    public class Caches
    {
        static IDictionary<string, object> data=new Dictionary<string,object>();

        public static void put(string key,object val)
        {
            data[key] = val;
        }

        public static T get<T>(string key)
        {
            return (T)data[key];
        }

    }
}
