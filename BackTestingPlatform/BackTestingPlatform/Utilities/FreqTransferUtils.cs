using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;

namespace BackTestingPlatform.Utilities
{
    /// <summary>
    /// 股票行情数据的降采样
    /// 周期转换
    ///（1）tick数据转换为秒级别数据（任意时间切片）;（2）tick数据转换为分钟级别数据（任意时间切片）;
    ///（3）tick数据转换为其他任意更低频的数据;（4）分钟数据转换为其他任意更低频的数据（N小时线、N日线等等）
    ///（5）日线数据转换为其他任意更低频的数据（N周线、N月线、N季线、N年线等等）
    /// </summary>
    public static class FreqTransferUtils
    {
        /// <summary>
        /// 股票tick,3秒切片数据转化我Nmin周期K线数据
        /// </summary>
        /// <param name="stockTickList"></param>股票tick数据的list
        /// <param name="freq"></param>需要转换的分钟频率值，如5为5分钟频率
        /// <returns></returns>
        public static List<StockMinuteData> tickToNMinutes(List<TickFromMssql> stockTickList, int freq)
        {
            /*
                public class TickFromMssql
              {
                  public string code;
                  public int date,time;
                  public double lastPrice;
                  public PositionData[] ask, bid;
                  public double highPrice, lowPrice, volume, turnoverVolume;
                  public double preClose,preSettle;
               }
                public class StockMinuteData
                 {
                     public DateTime time { get; set; }
                     public double open { get; set; }
                     public double high { get; set; }
                     public double low { get; set; }
                     public double close { get; set; }
                     public double volume { get; set; }
                     public double amount { get; set; }
                 }
             * */
            //sql中的时间戳数据记录在time（int型）中，精确到毫秒级，故从小时开始有7位，如093000000
            int startTime = 093100000;
            int endTime = 150100000;//实际不会有到15：01的数据，由于部分数据在15：00以后，故此处的截取点推后
            int freqSpan = freq * 100000;//周期时间跨度，Bar之间的时间距离（毫秒）

            List<StockMinuteData> newMinuteData = new List<StockMinuteData>();

            int headIndex;//bar的起点时间
            int tailIndex = stockTickList.FindIndex(s => s.time >= startTime);//第一个bar的终点时间

            for (int nowTick = 0; nowTick < stockTickList.Count;nowTick ++ )
            {
                int nowTime = stockTickList[nowTick].time / 100000 * 100000;//现在时刻,去除分钟以下的时间信息
                int nowDate = stockTickList[nowTick].date;//现在日期
                int nexTime = nowTime / 100000 * 100000 + freqSpan;//去除分钟以下的时间信息
                Console.WriteLine("Date:{0} Time:{1}", nowDate, nowTime);
       
                double nowOpen, nowHigh, nowLow, nowClose, nowVolume=0, nowAmt=0;

                if (nowTime < 093100000)
                    if (nowTime == 092500000)
                    {
                        headIndex = nowTick;//记下第一个起点，9：25                       
                        continue;
                    }                       
                    else
                        continue;
                else if (nowTime == 093100000)
                {
                    headIndex = nowTick;
                    tailIndex = stockTickList.FindLastIndex(s => s.time < nexTime & s.date == nowDate);
                }
                else if (nowTime > 113000000 & nowTime < 130000000)
                    continue;
                else if (nowTime > 150000000)
                    continue;
                else
                {
                    headIndex = nowTick;//起点的索引
                    tailIndex = stockTickList.FindLastIndex(s => s.time < nexTime & s.date == nowDate);//同一天内下一时刻之前的最后一个tick为末尾
                }
                nowOpen = stockTickList[headIndex].lastPrice;
                nowHigh = stockTickList[headIndex].lastPrice;
                nowLow = stockTickList[headIndex].lastPrice;
                for (int sign = headIndex; sign <= tailIndex; sign++)
                {
                    if (stockTickList[sign].lastPrice > nowHigh)
                        nowHigh = stockTickList[sign].lastPrice;
                    if (stockTickList[sign].lastPrice < nowLow)
                        nowLow = stockTickList[sign].lastPrice;
                    nowVolume += stockTickList[sign].volume;
                    nowAmt += stockTickList[sign].turnoverVolume;                            
                }
                nowClose = stockTickList[tailIndex].lastPrice;
                //记录新的bar数据
                int fullTime = nowDate * 1000000000 + nowTime;
                newMinuteData.Add(new StockMinuteData
                    {
                        time = Kit.toDateTime(nowDate, nowTime),
                        open =nowOpen,
                        high = nowHigh,
                        low = nowLow,
                        close = nowClose,
                        volume = nowVolume,
                        amount = nowAmt
                    });
               
                nowTick = tailIndex + 1; //下一个bar的起点        
            }
            return newMinuteData;
        }

    }
}
