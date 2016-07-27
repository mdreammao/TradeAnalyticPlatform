using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    interface TickDataRepository
    {
        List<TickData> findAll();
        List<TickData> findByStockCode(string stockCode);


    }
    class TickDataRepositoryFromWind : TickDataRepository
    {
        public List<TickData> findAll()
        {
            throw new NotImplementedException();
        }

        public List<TickData> findByStockCode(string stockCode)
        {
            throw new NotImplementedException();
        }
    }
}
