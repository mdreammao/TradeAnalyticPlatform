using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Stock;
using Autofac;
using BackTestingPlatform.Model.Signal;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.AccountOperator.Minute;


namespace BackTestingPlatform.Strategies.Stock.StockSample
{
    public class MABreak
    {
        //构造函数中需要传入回测的起始时间和需要进行回测的标的代码
        private DateTime startDate, endDate;
        private string stockCode = "";

        //设置两个MA的周期
        private int MA1 = -1, MA2 = -2;

        //回测参数设置，initialCapital为账户初始资本金，slipPoint为每笔成交的滑移价差比率
        private double initialCapital = 10000000;
        private double slipPoint = 0.005;

        //记录当前持仓数量
        private double volumeNow = 0;

        //初始化log工具
        static Logger log = LogManager.GetCurrentClassLogger();

        //构造函数
        public MABreak(int start, int end, string code, int ma1, int ma2)
        {
            startDate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
            stockCode = code;
            
            //默认MA1是比较小的周期
            if (MA1 < MA2)
                MA1 = ma1;
            else
                MA2 = ma2;
        }

        public int compute()
        {
            //如果MA周期不对，直接返回
            if(MA1 < 0 || MA2 < 0)
            {
                log.Info("MA周期出错，MA1:{0}, MA2:{1}", MA1, MA2);
                return -1;
            }

            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startDate), Kit.ToInt_yyyyMMdd(endDate));

            //将来可以把这些初始化操作从程序中分离，写在外面
            ///账户初始化
            //初始化positions
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.totalAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();

            ///数据准备
            //日线数据准备，取全回测期的数据存放于data
            List<StockDaily> stockData = new List<StockDaily>();
            stockData = Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave(stockCode, startDate, endDate);

            //建立close price数组，从stockData里面取出close price
            int stockData_length = stockData.Count;
            double[] closePrice = new double[stockData_length];
            for (int count = 0; count < stockData_length; ++count)
                closePrice[count] = stockData[count].close;

            //取两个MA的数组
            double[] MA1_array = MA.compute(closePrice, MA1);
            double[] MA2_array = MA.compute(closePrice, MA2);

            //****数据准备完毕回测开始******
            log.Info("数据准备完毕回测开始");

            //交易开关设置，控制day级的交易开关，开始时只能开仓，不能平仓
            bool tradingOn = true;//总交易开关
            bool openingOn = true;//开仓开关
            bool closingOn = false;//平仓开关

            //定义交易信号数组
            Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();
            //获得交易信号
            for (int count = MA1; count < stockData_length; ++count)
            {
                //获取当前时间，供回测信号使用
                DateTime timeNow = stockData[count].time;

                //分钟数据准备，做交易执行使用
                var minuteData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(stockCode, timeNow.AddDays(1));
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                dataToday.Add(stockCode, minuteData.Cast<KLine>().ToList());

                //上穿买入信号
                if ((MA1_array[count] > MA1_array[count]) && openingOn && tradingOn && (volumeNow > 0))
                {
                    MinuteSignal openSignal = new MinuteSignal()
                    {
                        code = stockCode,
                        volume = myAccount.totalAssets / MA1_array[count],
                        time = timeNow, tradingVarieties = "stock",
                        price = MA1_array[count],
                        minuteIndex = 0
                    };
                    openingOn = false;
                    closingOn = true;
                    volumeNow = myAccount.totalAssets / MA1_array[count];
                    signal.Add(stockCode, openSignal);


                    //开仓下单
                    MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, ref myAccount, slipPoint: slipPoint, now: timeNow.AddDays(1), capitalVerification: false);
                }               

                ////下穿卖出信号
                if ((MA1_array[count] < MA1_array[count]) && closingOn && tradingOn)
                {
                    MinuteSignal closeSignal = new MinuteSignal()
                    {
                        code = stockCode,
                        volume = 0 - volumeNow,
                        time = timeNow,
                        tradingVarieties = "stock",
                        price = MA1_array[count],
                        minuteIndex = 0
                    };
                    openingOn = false;
                    closingOn = true;
                    volumeNow = 0;
                    signal.Add(stockCode, closeSignal);                    

                    //平仓下单
                    MinuteCloseAllPositonsWithSlip.closeAllPositions(dataToday, ref positions, ref myAccount, timeNow.AddDays(1), slipPoint);
                }

                //账户信息更新
                AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, timeNow.AddDays(1), dataToday);
            }

            return 0;

        }
    }
}
