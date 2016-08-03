using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BackTestingPlatform.Model;
using BackTestingPlatform.DataAccess.Option;

namespace BackTestingPlatform.Service
{
    public class OptionService
    {
       
        OptionInfoRepository optionInfoRepository = Platforms.container.Resolve<OptionInfoRepository>();


        public void readOptionInfo(string underlyingCode = "510050.SH", string market = "sse")
        {

            var optionInfos=optionInfoRepository.fetchFromWind(underlyingCode, market);
            optionInfoRepository.saveToLocalFile(optionInfos);



        }

    }
}
