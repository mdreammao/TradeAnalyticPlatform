using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.AccountOperator.Minute.maoheng
{
    public class AccountUpdatingWithMinuteBar
    {
        //初始化log组件
        static Logger log = LogManager.GetCurrentClassLogger();
        public static void computeAccount(ref BasicAccount myAccount, SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions, DateTime now, Dictionary<string, List<KLine>> data)
        {
            myAccount.time = now;
            //若position为null，直接跳过
            if (positions.Count == 0)
            {
                return;
            }
            //提取初始资产
            double initialCapital = myAccount.initialAssets;
            Dictionary<string, PositionsWithDetail> nowPosition = new Dictionary<string, PositionsWithDetail>();
            nowPosition = positions[positions.Keys.Last()];
            //初始化保证金，可用现金
            double totalMargin = 0;
            double totalCashFlow = 0;
            double totalPositionValue = 0;
            double totalAssets = 0;
            //当前时间对应data中timeList 的序号
            int index = TimeListUtility.MinuteToIndex(now);
            if (index < 0)
            {
                log.Warn("Signal时间出错，请查验");
                return;
            }
            foreach (var item in nowPosition)
            {
                PositionsWithDetail position0 = item.Value;
                if (position0.volume!=0)
                {
                    double price = data[position0.code][index].close;
                    totalPositionValue += price * position0.volume;
                }
                totalCashFlow += position0.totalCashFlow;
            }
            myAccount.totalAssets = initialCapital + totalCashFlow + totalPositionValue;
            myAccount.freeCash = initialCapital + totalCashFlow - totalMargin;
            myAccount.margin = totalMargin;
            myAccount.positionValue = totalPositionValue;
        }
    }
}
