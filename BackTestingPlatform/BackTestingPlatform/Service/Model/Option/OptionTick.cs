using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{
 
   public class OptionTickFromMssqlWithInfo : OptionTickFromMssql
    {
       public OptionInfo basicInfo { get; set; }
    }

    public class OptionTickFromMssql : TickFromMssql
    {
       
    }


}
