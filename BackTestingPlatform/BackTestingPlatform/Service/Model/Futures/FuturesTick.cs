using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Futures
{
    public class FuturesTickFromMssql : TickFromMssql
    {

    }
    public class FuturesTickFromMssqlWithInfo : FuturesTickFromMssql
    {
        public FuturesInfo basicInfo { get; set; }
    }
    
}
