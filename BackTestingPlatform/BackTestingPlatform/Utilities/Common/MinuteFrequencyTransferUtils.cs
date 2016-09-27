using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Futures;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Transaction;
using BackTestingPlatform.Transaction.TransactionWithSlip;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using BackTestingPlatform.Utilities.TimeList;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Strategies.Stock.StockSample;
using BackTestingPlatform.Strategies.Stock.StockSample01;
using System.Globalization;

namespace BackTestingPlatform.Utilities.Common
{
    /// <summary>
    /// 分钟股票行情数据的降采样
    /// 周期转换
    ///（1）分钟数据转换为其他任意更低频的数据（N小时线、N日线等等）
    ///（2）日线数据转换为其他任意更低频的数据（N周线、N月线、N季线、N年线等等）
    ///（3）暂时只支持转化为1日、1月、1周k线
    /// minuteData数据要求：
    /// （1）必须是以天为单位完整的minute数据，不接受起点终点不是日开始或结束的数据
    /// （2）暂不支持特定周期转特定周期，目前只支持1min转换为特定周期
    /// （3）读入分钟数据时间标记为靠左，日开盘9:30，早收盘11:29，,午开盘13:00，午收盘14:59
    /// （4）按习惯改为时间靠右标记，以五分钟为例，日开盘9:35，早收盘11:30，,午开盘13:05，午收盘15:00
    /// （5）1分钟转N分钟，N要求小于等于120
    /// </summary>
    public static class MinuteFrequencyTransferUtils
    {
        /// <summary>
        /// 1分钟数据转化为N-Period的K线数据，N分钟K线数据时间标记为靠右，以结束时间为标记
        /// </summary>
        /// <param name="minuteData"></param>一般是1min数据
        /// <param name="period"></param>周期字符串，如"Minutely","Daily","Monthly"
        /// <param name="frequency"></param>
        /// <returns></returns>
        public static List<KLine> MinuteToNPeriods(List<KLine> minuteData, string period, int frequency)
        {
            List<KLine> newKLineData = new List<KLine>();
            //1分钟转化为N分钟K线
            if (period.Equals("Minutely"))
            {
                MinuteToNMinutes(minuteData, ref newKLineData, period, frequency);
            }
            //1分钟转化为N日K线
            else if (period.Equals("Daily"))
            {
                MinuteToNDays(minuteData, ref newKLineData, period, frequency);
            }
            //1分钟转化为N周K线
            else if (period.Equals("Weekly"))
            {
                MinuteToNWeeks(minuteData, ref newKLineData, period, frequency);
            }
            //1分钟转化为N月K线
            else if (period.Equals("Monthly"))
            {
                MinuteToNMonths(minuteData, ref newKLineData, period, frequency);
            }
            return newKLineData;

        }

        /// <summary>
        /// 1分钟转N分钟数据
        /// </summary>
        /// <param name="minuteData"></param>
        /// <param name="newMinuteData"></param>
        /// <param name="period"></param>
        /// <param name="frequency"></param>
        private static void MinuteToNMinutes(List<KLine> minuteData, ref List<KLine> newMinuteData, string period, int frequency)
        {
            //每根bar的头尾索引
            int headIndex = 0, tailIndex = 0;
            //每根bar的头尾1分钟时间戳
            DateTime headStamp, tailStamp;
            for (int nowIndex = 0; nowIndex < minuteData.Count; nowIndex = tailIndex + 1)
            {
                DateTime nowTime = minuteData[nowIndex].time;
                headStamp = nowTime;
                //所有频率的每天第一根bar均包含0930，多一分钟      
                if (nowTime.Hour == 9 && nowTime.Minute == 30 || nowTime.Hour == 13 && nowTime.Minute == 0)
                    tailStamp = headStamp.AddMinutes(frequency);
                else
                    tailStamp = headStamp.AddMinutes(frequency - 1);

                headIndex = nowIndex;
                //在起止时间戳间的最后一根1分钟bar
                tailIndex = minuteData.FindLastIndex(s => s.time >= headStamp && s.time <= tailStamp);
                //记录新频率的k线
                newMinuteData.Add(new KLine
                {
                    time = tailStamp,
                    open = minuteData[headIndex].open,
                    high = minuteData.Max(s => s.high),
                    low = minuteData.Min(s => s.low),
                    close = minuteData[tailIndex].close,
                    volume = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.volume),
                    amount = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.amount),
                    openInterest = minuteData[tailIndex].openInterest
                });
            }
            /*
            foreach (var nowbar in newMinuteData)
            {
                Console.WriteLine("time:{0,8:F},close:{4,8:F3},volume:{5,8:F}\n", nowbar.time, nowbar.open, nowbar.high, nowbar.low, nowbar.close, nowbar.volume, nowbar.amount);
            }
            */
        }

        /// <summary>
        /// 1分钟转N日数据
        /// </summary>
        /// <param name="minuteData"></param>
        /// <param name="newMinuteData"></param>
        /// <param name="period"></param>
        /// <param name="frequency"></param>
        private static void MinuteToNDays(List<KLine> minuteData, ref List<KLine> newMinuteData, string period, int frequency)
        {
            int headIndex = 0, tailIndex = 0;
            //每根bar的头尾1分钟时间戳
            DateTime headStamp = new DateTime();
            DateTime tailStamp = new DateTime();
            for (int nowIndex = 0; nowIndex < minuteData.Count; nowIndex = tailIndex + 1)
            {
                DateTime nowTime = minuteData[nowIndex].time;
                headStamp = nowTime;
                tailStamp = headStamp;
                tailStamp = new DateTime(headStamp.Year, headStamp.Month, headStamp.Day, 15, 0, 0);
                headIndex = nowIndex;
                tailIndex = minuteData.FindLastIndex(s => s.time >= headStamp && s.time <= tailStamp);
                //记录新频率的k线
                newMinuteData.Add(new KLine
                {
                    time = tailStamp,
                    open = minuteData[headIndex].open,
                    high = minuteData.Max(s => s.high),
                    low = minuteData.Min(s => s.low),
                    close = minuteData[tailIndex].close,
                    volume = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.volume),
                    amount = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.amount),
                    openInterest = minuteData[tailIndex].openInterest
                });
            }

        }

        /// <summary>
        /// 1分钟转N周数据
        /// </summary>
        /// <param name="minuteData"></param>
        /// <param name="newMinuteData"></param>
        /// <param name="period"></param>
        /// <param name="frequency"></param>
        private static void MinuteToNWeeks(List<KLine> minuteData, ref List<KLine> newMinuteData, string period, int frequency)
        {
            int headIndex = 0, tailIndex = 0;
            //每根bar的头尾1分钟时间戳
            DateTime headStamp = new DateTime();
            DateTime tailStamp = new DateTime();
            for (int nowIndex = 0; nowIndex < minuteData.Count; nowIndex = tailIndex + 1)
            {
                DateTime nowTime = minuteData[nowIndex].time;
                headStamp = nowTime;
                headIndex = nowIndex;
                //同一年同一周的最后一个索引
                tailIndex = minuteData.FindLastIndex(s => s.time.Year == headStamp.Year && GetWeekOfYear(s.time) == GetWeekOfYear(headStamp) );
                tailStamp = headStamp;
                tailStamp = minuteData[tailIndex].time;
                //记录新频率的k线
                newMinuteData.Add(new KLine
                {
                    time = tailStamp,
                    open = minuteData[headIndex].open,
                    high = minuteData.Max(s => s.high),
                    low = minuteData.Min(s => s.low),
                    close = minuteData[tailIndex].close,
                    volume = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.volume),
                    amount = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.amount),
                    openInterest = minuteData[tailIndex].openInterest
                });
            }

        }
        /// <summary>
        /// 1分钟转N月数据
        /// </summary>
        /// <param name="minuteData"></param>
        /// <param name="newMinuteData"></param>
        /// <param name="period"></param>
        /// <param name="frequency"></param>
        private static void MinuteToNMonths(List<KLine> minuteData, ref List<KLine> newMinuteData, string period, int frequency)
        {
            int headIndex = 0, tailIndex = 0;
            //每根bar的头尾1分钟时间戳
            DateTime headStamp = new DateTime();
            DateTime tailStamp = new DateTime();
            for (int nowIndex = 0; nowIndex < minuteData.Count; nowIndex = tailIndex + 1)
            {
                DateTime nowTime = minuteData[nowIndex].time;
                headStamp = nowTime;
                headIndex = nowIndex;
                tailIndex = minuteData.FindLastIndex(s => s.time.Month == headStamp.Month);
                tailStamp = headStamp;
                tailStamp = minuteData[tailIndex].time;

                //记录新频率的k线
                newMinuteData.Add(new KLine
                {
                    time = tailStamp,
                    open = minuteData[headIndex].open,
                    high = minuteData.Max(s => s.high),
                    low = minuteData.Min(s => s.low),
                    close = minuteData[tailIndex].close,
                    volume = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.volume),
                    amount = minuteData.Where(s => s.time >= headStamp && s.time <= tailStamp).Sum(x => x.amount),
                    openInterest = minuteData[tailIndex].openInterest
                });
            }

        }

        /// <summary>
        /// 获取指定日期是一年中的第几周
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static int GetWeekOfYear(DateTime dt)
        {
            GregorianCalendar gc = new GregorianCalendar();
            int weekOfYear = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return weekOfYear;
        }


    }
}
