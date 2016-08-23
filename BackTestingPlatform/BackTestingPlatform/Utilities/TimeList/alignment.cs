using BackTestingPlatform.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities.TimeList
{
    public static class Alignment
    {
        public static List<T> AlignmentOfTickData<T>(List<T> list) where T: Sequential, new ()
        {
            T[] listAfterModified = new T[28802];
            foreach (var item in list)
            {
                int index = TimeListUtility.ToTickIndex(item.time)-1;
                listAfterModified[index] = item;
            }
            for (int i = 1; i < 28802; i++)
            {
                if (listAfterModified[i]==null)
                {
                    listAfterModified[i] = listAfterModified[i - 1];
                }
            }
            return listAfterModified.ToList();
        }
    }
}
