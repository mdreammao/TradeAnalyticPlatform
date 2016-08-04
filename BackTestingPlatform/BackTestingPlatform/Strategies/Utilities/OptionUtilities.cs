using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model.Option;

namespace BackTestingPlatform.Strategies.Utilities
{
    public static class OptionUtilities
    {
        public static List<OptionInfo> getOptionListDailiy(List<OptionInfo> list,int date)
        {
            List<OptionInfo> listToday = new List<OptionInfo>();
            foreach (var item in list)
            {
                int start = Convert.ToInt32(item.startDate.ToString("yyyyMMdd"));
                int end = Convert.ToInt32(item.endDate.ToString("yyyyMMdd"));
                if (date>=start && end>=date)
                {
                    listToday.Add(item);
                }
            }
            return listToday;
        }
    }
}
