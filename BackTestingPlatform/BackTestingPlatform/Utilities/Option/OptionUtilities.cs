using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model.Option;

namespace BackTestingPlatform.Utilities.Option
{
    public static class OptionUtilities
    {

        public static List<OptionInfo> getOptionListByOptionType(List<OptionInfo> list, string type)
        {
            return list.FindAll(delegate (OptionInfo item)
            {
                if (item.optionType == type)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            );
        }
        public static List<OptionInfo> getOptionListByStrike(List<OptionInfo> list, double strikeLower,double strikeUpper)
        {
            return list.FindAll(delegate (OptionInfo item)
            {
                if (item.strike >= strikeLower && item.strike<=strikeUpper)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            );
        }

        public static List<OptionInfo> getOptionListByStrike(List<OptionInfo> list,double strike )
        {
            return list.FindAll(delegate (OptionInfo item)
            {
                if (item.strike==strike)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            );
        }
        public static List<OptionInfo> getOptionListByDate(List<OptionInfo> list, int firstDay,int lastDay)
        {
            return list.FindAll(delegate (OptionInfo item)
            {
                if (Convert.ToInt32(item.startDate.ToString("yyyyMMdd"))<=lastDay && Convert.ToInt32(item.endDate.ToString("yyyyMMdd")) >= firstDay)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            );
        }

        public static List<OptionInfo> getOptionListByDate(List<OptionInfo> list, int date)
        {
            return list.FindAll(delegate (OptionInfo item)
            {
                if (Convert.ToInt32(item.startDate.ToString("yyyyMMdd")) <= date && Convert.ToInt32(item.endDate.ToString("yyyyMMdd")) >= date)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            );
        }
    }

   
}
