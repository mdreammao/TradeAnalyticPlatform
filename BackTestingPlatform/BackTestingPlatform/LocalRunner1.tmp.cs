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
using BackTestingPlatform.DataAccess.Option;
using WAPIWrapperCSharp;

namespace BackTestingPlatform
{
    class LocalRunner
    {
        public void run()
        {
            Strategy stg = new SingleMA();
            stg.act();
            /*
            OptionInfoRepositoryFromWind myOption = new OptionInfoRepositoryFromWind();
            myOption.fetchAll();
             */
            //sfsdf/s/s/sd
        }
    }
}
