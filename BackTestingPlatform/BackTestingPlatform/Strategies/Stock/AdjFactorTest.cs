using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Service;
using BackTestingPlatform.Service.Stock;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Stock
{
    public class AdjFactorTest
    {
        public AdjFactorTest(string stockCode, int start, int end)
        {
            var days = Caches.getTradeDays();
            AdjFactorService adjFactorService = Platforms.container.Resolve<AdjFactorService>();
            adjFactorService.loadAdjFactor(stockCode, Kit.ToDate(start), Kit.ToDate(end));
            var adjFactor = Caches.get("AdjFactor");
            days = DateUtils.GetTradeDays(start, end);
            //        for(int j=0 ;j<adjFactor)
            //       Console.WriteLine("{0}",)

        }
    }
}
