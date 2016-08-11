using BackTestingPlatform.Model.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BackTestingPlatform.DataAccess.Stock
{
    class StockDailyInfoRepository : SequentialDataRepository<StockDailyInfo>
    {
        public override List<StockDailyInfo> fetchFromDefaultMssql(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        public override List<StockDailyInfo> fetchFromWind(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

    }
}
