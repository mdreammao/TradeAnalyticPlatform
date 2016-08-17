using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BackTestingPlatform.Model;

namespace BackTestingPlatform.Service.Stock
{
    public class StockService
    {
        
        StockTickRepository stockTickRepository = Platforms.container.Resolve<StockTickRepository>();



    }
}
