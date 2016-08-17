using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Common;
using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Service.Common
{
    class TradeDaysService
    {
        TradeDayRepository tradeDayRepository = Platforms.container.Resolve<TradeDayRepository>();

        public List<DateTime> fetchFromLocalCsvOrWindAndUpdateAndCache(int localCsvExpiration = 180, bool appendMode = false, String tag = "TradeDays")
        {
            return tradeDayRepository.fetchFromLocalCsvOrWindAndUpdateAndCache(localCsvExpiration, appendMode, tag);
        }

    }
}
