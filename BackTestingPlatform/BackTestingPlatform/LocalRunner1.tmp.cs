using Autofac;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Strategies;
using BackTestingPlatform.Strategies.SingleMA;

namespace BackTestingPlatform
{
    class LocalRunner
    {
        public void run()
        {
            Strategy stg = new SingleMA();
            stg.act();

            //sfsdf/s/s/sd
        }
    }
}
