using BackTestingPlatform.DataAccess.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.Futures
{
    public class FuturesInfoRepository : BasicDataRepository<FuturesInfo>
    {
        protected override List<FuturesInfo> readFromWind()
        {
            throw new NotImplementedException();
        }
    }
}
