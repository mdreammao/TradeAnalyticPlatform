using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    interface ITickPositionDataDAO
    {
        List<KLinesDataRepository> findAll();
        List<KLinesDataRepository> findByStockCode(string stockCode);


    }
    class TickPositionDataWindDAO : ITickPositionDataDAO
    {
        public List<KLinesDataRepository> findAll()
        {
            throw new NotImplementedException();
        }

        public List<KLinesDataRepository> findByStockCode(string stockCode)
        {
            throw new NotImplementedException();
        }
    }
}
