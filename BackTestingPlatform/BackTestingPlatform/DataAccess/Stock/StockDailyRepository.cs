using BackTestingPlatform.Model.Stock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace BackTestingPlatform.DataAccess.Stock
{
    class StockDailyRepository : SequentialDataRepository<StockDaily>
    {
        public override List<StockDaily> fetchFromDefaultMssql(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        public override List<StockDaily> fetchFromWind(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

    }
}
