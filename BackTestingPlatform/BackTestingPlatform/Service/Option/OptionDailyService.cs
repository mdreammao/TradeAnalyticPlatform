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
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Common;
using System.IO;
using System.Data;

namespace BackTestingPlatform.Service.Option
{
   


    public class OptionDailyService
    {

        OptionDailyRepository optionDailyRepository = Platforms.container.Resolve<OptionDailyRepository>();



    }
}
