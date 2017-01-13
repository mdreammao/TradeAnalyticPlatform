using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.Model.Futures;
using BackTestingPlatform.Utilities.DataApplication;

namespace BackTestingPlatform.Utilities.GetData.Futures
{
    public class GetDataFromWind
    {
        public static List<FuturesMinute> GetFutureDataFromWind(int startDate, int endDate, string underlying)
        {
            //取到的数据
            List<FuturesMinute> data = new List<FuturesMinute>();
            //交易日列表
            List<DateTime> tradeDays = new List<DateTime>();

            DateTime startDateTime= Kit.ToDate(startDate);
            DateTime endDateTime = Kit.ToDate(endDate);
          
            tradeDays= DateUtils.GetTradeDays(startDateTime, endDateTime);
            for (int i = 0; i < tradeDays.Count; i++)
            {
                DateTime today = tradeDays[i];
                var dataToday = KLineDataUtils.leakFilling(Platforms.container.Resolve<FuturesMinuteRepository>().fetchFromLocalCsvOrWindAndSave(underlying, today));
                data.AddRange(dataToday);
                Console.WriteLine("获取"+ underlying +" "+today.ToShortDateString() + " " + "数据......");
            }
            return data;
        }
    }
}
