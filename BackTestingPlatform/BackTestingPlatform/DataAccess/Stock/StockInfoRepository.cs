using BackTestingPlatform.Model.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BackTestingPlatform.DataAccess.Common;

namespace BackTestingPlatform.DataAccess.Stock
{
    class StockInfoRepository : BasicDataRepository<StockInfo>
    {
        protected override List<StockInfo> readFromWind()
        {
            throw new NotImplementedException();
        }

    }
}
