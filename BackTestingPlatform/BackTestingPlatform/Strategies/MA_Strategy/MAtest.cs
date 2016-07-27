using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.MA_Strategy
{
    class MATest
    {
       public void MainStrategy()
       {
           GetData myData = new GetData();
           myData.Execute();   
       }
    }
}
