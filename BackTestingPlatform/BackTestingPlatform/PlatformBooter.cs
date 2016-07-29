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
using BackTestingPlatform.Strategies.SingleMA;

namespace BackTestingPlatform
{
    class PlatformBooter
    {
        static void Main(string[] args)
        {          
            
            Platforms.Initialize(); //初始化
            (new LocalRunner()).run();   
           

            Platforms.ShutDown();   //关闭
        }
    }
}
