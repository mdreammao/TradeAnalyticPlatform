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
        public override List<StockDailyInfo> fetchFromDefaultMssql(string code, string date)
        {
            throw new NotImplementedException();
        }

        public override List<StockDailyInfo> fetchFromWind(string code, string date)
        {
            throw new NotImplementedException();
        }

        public override StockDailyInfo toEntity(DataRow row)
        {
            throw new NotImplementedException();
        }
    }
}
