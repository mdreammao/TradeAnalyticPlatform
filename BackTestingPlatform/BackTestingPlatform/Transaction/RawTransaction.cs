using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction
{
    public static class RawTransaction
    {
        public static DateTime computeMinutePositions(Dictionary<string,BasicSignal> signal,Dictionary<string,object> data,ref Dictionary<string,List<BasicPositions>> positions)
        {
            DateTime next = new DateTime();

            return next;
        }
    }
}
