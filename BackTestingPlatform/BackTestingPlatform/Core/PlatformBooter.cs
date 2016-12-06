using Autofac;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BackTestingPlatform.Strategies;

namespace BackTestingPlatform
{
    class PlatformBooter
    {
        static void Main(string[] args)
        {
            var t1 = DateTime.Now;
            Platforms.Initialize(); //初始化          
            Platforms.ShutDown();  //关闭
            var te = DateTime.Now - t1;     
        }
    }
}
