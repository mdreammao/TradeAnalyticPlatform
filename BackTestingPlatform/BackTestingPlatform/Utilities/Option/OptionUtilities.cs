using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model.Option;

namespace BackTestingPlatform.Utilities.Option
{
    static class OptionUtilities
    {

        public static List<OptionDaily> getOptionListByOptionType(List<OptionDaily> list, string type)
        {
            return list.FindAll(delegate (OptionDaily item)
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
        public static List<OptionDaily> getOptionListByStrike(List<OptionDaily> list, double strikeLower,double strikeUpper)
        {
            return list.FindAll(delegate (OptionDaily item)
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

        public static List<OptionDaily> getOptionListByStrike(List<OptionDaily> list,double strike )
        {
            return list.FindAll(delegate (OptionDaily item)
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
        public static List<OptionDaily> getOptionListByDate(List<OptionDaily> list, int firstDay,int lastDay)
        {
            return list.FindAll(delegate (OptionDaily item)
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

        public static List<OptionDaily> getOptionListByDate(List<OptionDaily> list, int date)
        {
            return list.FindAll(delegate (OptionDaily item)
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
