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

namespace BackTestingPlatform.Utilities.Common
{
    /// <summary>
    /// 分钟股票行情数据的降采样
    /// 周期转换
    ///（1）分钟数据转换为其他任意更低频的数据（N小时线、N日线等等）
    ///（2）日线数据转换为其他任意更低频的数据（N周线、N月线、N季线、N年线等等）
    /// minuteData数据要求：
    /// （1）必须是以天为单位完整的minute数据，不接受起点终点不是日开始或结束的数据
    /// 
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
        private static void MinuteToNMinutes(List<KLine> minuteData, ref List<KLine> newMinuteData,string period, int frequency)
        {


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

        }


    }
}
