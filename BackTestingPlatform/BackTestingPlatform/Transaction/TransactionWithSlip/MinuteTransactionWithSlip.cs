using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Transaction.TransactionWithSlip
{
    public static class MinuteTransactionWithSlip
    {
        public static DateTime computeMinutePositions(Dictionary<string, MinuteSignal> signal, Dictionary<string, List<KLine>> data, SortedDictionary<DateTime, Dictionary<string, MinutePositions>> positions)
        {
            DateTime next = new DateTime() ;

            return next;
        }
    }
}
