using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Model.Option
{
 
    public class OptionTick : Tick
    {               
        public OptionGreek greek;
        public Tick underlyingStock;
        public Tick underlyingFutures;

    }

    public struct OptionGreek
    {
        public double sigma, delta, gamma, vega, theta;
    }

   

}
