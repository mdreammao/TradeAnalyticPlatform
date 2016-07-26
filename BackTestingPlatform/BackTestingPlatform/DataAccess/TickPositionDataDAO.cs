using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    interface ITickPositionDataDAO
    {
        List<TickPositonData> findAll();
        List<TickPositonData> findByStockCode(string stockCode);


    }
    class TickPositionDataWindDAO : ITickPositionDataDAO
    {
        public List<TickPositonData> findAll()
        {
            throw new NotImplementedException();
        }

        public List<TickPositonData> findByStockCode(string stockCode)
        {
            throw new NotImplementedException();
        }
    }
}
