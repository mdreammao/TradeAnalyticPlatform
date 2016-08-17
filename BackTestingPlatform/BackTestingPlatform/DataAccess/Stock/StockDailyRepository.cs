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
        protected override List<StockDaily> readFromDefaultMssql(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override List<StockDaily> readFromWind(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

    }
}
