using BackTestingPlatform.Core;
using BackTestingPlatform.Model.Futures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.DataAccess.Futures
{
    public class FuturesMinuteRepository : SequentialByDayRepository<FuturesMinute>
    {
        protected override List<FuturesMinute> readFromDefaultMssql(string code, DateTime date)
        {
            throw new NotImplementedException();
        }

        protected override List<FuturesMinute> readFromWind(string code, DateTime date)
        {
            List<FuturesMinute> items = new List<FuturesMinute>();
            string[] str = code.Split('.');
            if (str[1]=="CFE")
            {
                return readByParameters(code, date, "periodstart=09:30:00;periodend=15:00:00;Fill=Previous");
            }
            if (str[1]=="SHF")
            {

                DateTime modifiedTime1 = new DateTime(2014, 12, 26);
                var nightData = readByParameters(code, date, "periodstart=21:00:00;periodend=23:00:00;Fill=Previous");
                DateTime modifiedTime2 = new DateTime(2016, 5, 3);
                if (date>=modifiedTime1 && date<modifiedTime2)
                {
                    var nightData2= readByParameters(code, date, "periodstart=23:00:01;periodend=23:59:59;Fill=Previous");
                    var nightData3= readByParameters(code, date, "periodstart=00:00:00;periodend=01:00:00;Fill=Previous");
                    nightData.AddRange(nightData2);
                    nightData.AddRange(nightData3);
                }
                var dayData = readByParameters(code, date, "periodstart=09:00:00;periodend=15:00:00;Fill=Previous");
                nightData.AddRange(dayData);
                return nightData;
            }
            return items;
        }

        private List<FuturesMinute> readByParameters(string code, DateTime date,string paramters)
        {
            WindAPI w = Platforms.GetWindAPI();
            DateTime date2 = new DateTime(date.Year, date.Month, date.Day, 15, 0, 0);
            DateTime date1 = date2.AddDays(-1).AddHours(2);
            //获取日盘数据
            WindData wd = w.wsi(code, "open,high,low,close,volume,amt,oi", date1, date2, paramters);
            int len = wd.timeList.Length;
            int fieldLen = wd.fieldList.Length;
            var items = new List<FuturesMinute>(len);
            if (wd.data is double[])
            {
                double[] dataList = (double[])wd.data;
                DateTime[] timeList = wd.timeList;
                for (int k = 0; k < len; k++)
                {
                    items.Add(new FuturesMinute
                    {
                        time = timeList[k],
                        open = (double)dataList[k * fieldLen + 0],
                        high = (double)dataList[k * fieldLen + 1],
                        low = (double)dataList[k * fieldLen + 2],
                        close = (double)dataList[k * fieldLen + 3],
                        volume = (double)dataList[k * fieldLen + 4],
                        amount = (double)dataList[k * fieldLen + 5],
                        openInterest = (double)dataList[k * fieldLen + 6]
                    });
                }
            }
            return items;
        }
    }
}
