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

        public static List<OptionInfo> getSpecifiedOption(List<OptionInfo>list,DateTime endDate,string type, double strike)
        {
            return list.FindAll(delegate (OptionInfo info)
            {
                if (info.optionType==type && info.strike==strike && info.endDate==endDate)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        public static List<DateTime> getEnddateListByAscending(List<OptionInfo> list)
        {
            List<DateTime> durationList = new List<DateTime>();
            foreach (var item in list)
            {
                if (durationList.Contains(item.endDate) == false)
                {
                    durationList.Add(item.endDate);
                }
            }
            return durationList.OrderBy(x => x).ToList();
        }

        public static List<double> getStrikeListByAscending(List<OptionInfo> list)
        {
            List<double> durationList = new List<double>();
            foreach (var item in list)
            {
                if (durationList.Contains(item.strike) == false)
                {
                    durationList.Add(item.strike);
                }
            }
            return durationList.OrderBy(x => x).ToList();
        }

        public static List<double> getDurationStructure(List<OptionInfo> list,DateTime today)
        {
            List<double> durationList = new List<double>();
            foreach (var item in list)
            {
                double duration = DateUtils.GetSpanOfTradeDays(today,item.endDate);
                if (durationList.Contains(duration)==false)
                {
                    durationList.Add(duration);
                }
            }
            return durationList.OrderBy(x=>x).ToList();
        }
        public static string getCorrespondingIHCode(OptionInfo info,int date)
        {
            
            DateTime today = Kit.ToDate(date);
            if (info.endDate <today || date<20150416)
            {
                return null;
            }
            if (Kit.ToInt_yyyyMMdd(info.endDate)<=20150430 && Kit.ToInt_yyyyMMdd(info.endDate)>=20150401)
            {
                return "IH1505.CFE";
            }
            DateTime IHExpirationDate =DateUtils.NextOrCurrentTradeDay(DateUtils.GetThirdFridayOfMonth(info.endDate));
            
            if (today<=IHExpirationDate)
            {
                return "IH" + IHExpirationDate.ToString("yyMM") + ".CFE";
            }
            else
            {
                return "IH" + IHExpirationDate.AddMonths(1).ToString("yyMM") + ".CFE";
            }

        }

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
