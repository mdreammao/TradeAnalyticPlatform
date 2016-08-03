using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BackTestingPlatform.Model;

namespace BackTestingPlatform.Service
{
    public class StockService
    {
        //ASharesInfoRepository aSharesInfoRepo = Platforms.container.Resolve<ASharesInfoRepository>();
        StockTickDataRepository stockTickDataRepository = Platforms.container.Resolve<StockTickDataRepository>();


        public List<RealTimeQuotes> readRealTimeQuotes(string stockCode,DateTime time)
        {
            return stockTickDataRepository.fetchRealTimeQuotesFromDatabase(stockCode,time);
        }

    }
}
