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
using BackTestingPlatform.Strategies.Sample;

namespace BackTestingPlatform
{
    class Program
    {
        static void Main(string[] args)
        {          
            Platforms.Initialize();
            Strategy stg = new TradeDaysDirectPrint();
            stg.act();
        }
    }
}
