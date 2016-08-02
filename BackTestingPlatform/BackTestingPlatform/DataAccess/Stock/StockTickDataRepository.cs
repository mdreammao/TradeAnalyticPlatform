using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    interface StockTickDataRepository
    {
        List<StockTickData> findAll();
        List<StockTickData> findByStockCode(string stockCode);


    }
    class StockTickDataRepositoryFromWind : StockTickDataRepository
    {
        public List<StockTickData> findAll()
        {
            throw new NotImplementedException();
        }

        public List<StockTickData> findByStockCode(string stockCode)
        {
            throw new NotImplementedException();
        }
    }

    class StockTickDataRepositoryFromLocalDb : StockTickDataRepository
    {
        public List<StockTickData> findAll()
        {
            throw new NotImplementedException();
        }

        public List<StockTickData> findByStockCode(string stockCode)
        {
            throw new NotImplementedException();
        }
    }
}
