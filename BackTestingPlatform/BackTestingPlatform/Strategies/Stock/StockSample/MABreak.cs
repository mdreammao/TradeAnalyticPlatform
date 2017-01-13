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
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Charts;
using BackTestingPlatform.Utilities.SaveResult.Common;
using BackTestingPlatform.Utilities.SaveResult.Option;
using System.Windows.Forms;


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
            if (ma1 < ma2)
            {
                MA1 = ma1;
                MA2 = ma2;
            }
            else
            {
                MA1 = ma2;
                MA2 = ma1;
            }
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
            //交易日信息
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startDate, endDate);

            ///账户初始化
            //初始化positions
            SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>> positions = new SortedDictionary<DateTime, Dictionary<string, PositionsWithDetail>>();
            //初始化Account信息
            BasicAccount myAccount = new BasicAccount();
            myAccount.totalAssets = initialCapital;
            myAccount.initialAssets = initialCapital;
            myAccount.freeCash = myAccount.totalAssets;
            //将账户当前时间定为下一天，因为交易总是在下一天开始
            //int nextDay = tradeDays.FindIndex(date => date == startDate) + 1;
            myAccount.time = startDate;
            
            
            //记录历史账户信息
            List<BasicAccount> accountHistory = new List<BasicAccount>();

            //benchmark数据记录
            List<double> benchmark = new List<double>();

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
                //找出下一个交易日
                int nextTradeDay = tradeDays.FindIndex(date => date == timeNow) + 1;
                if (nextTradeDay == stockData_length)
                    break;

                //分钟数据准备，做交易执行使用
                var minuteData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(stockCode, tradeDays[nextTradeDay]);
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                dataToday.Add(stockCode, minuteData.Cast<KLine>().ToList());

                //上穿买入信号
                if ((MA1_array[count] > MA2_array[count]) && openingOn && tradingOn)
                {
                    //设置signal信号，设置时间等参数
                    MinuteSignal openSignal = new MinuteSignal()
                    {
                        code = stockCode,
                        volume = myAccount.freeCash / MA1_array[count] * 0.9,
                        time = minuteData[0].time,
                        tradingVarieties = "stock",
                        price = MA1_array[count],
                        minuteIndex = 0
                    };
                    openingOn = false;
                    closingOn = true;
                    volumeNow = myAccount.freeCash / MA1_array[count] * 0.9;
                    signal.Add(stockCode, openSignal);

                    //开仓下单
                    MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions, 
                        ref myAccount, slipPoint: slipPoint, now: minuteData[0].time, capitalVerification: false);
                    signal.Clear();
                }               

                ////下穿卖出信号，当存量volumeNow大于0时做卖出操作
                if ((MA1_array[count] < MA2_array[count]) && closingOn && tradingOn && (volumeNow > 0))
                {
                    //设置signal信号，设置时间等参数
                    MinuteSignal closeSignal = new MinuteSignal()
                    {
                        code = stockCode,
                        volume = 0 - volumeNow,
                        time = minuteData[0].time,
                        tradingVarieties = "stock",
                        price = MA1_array[count],
                        minuteIndex = 0
                    };
                    openingOn = true;
                    closingOn = false;
                    volumeNow = 0;
                    signal.Add(stockCode, closeSignal);

                    //平仓下单
                    MinuteTransactionWithSlip.computeMinuteClosePositions(signal, dataToday, ref positions, 
                        ref myAccount, slipPoint: slipPoint, now: minuteData[0].time);
                    signal.Clear();
                }

                //将交易记录记录到历史
                BasicAccount tempAccount = new BasicAccount();
                tempAccount.time = myAccount.time;
                tempAccount.freeCash = myAccount.freeCash;
                tempAccount.margin = myAccount.margin;
                tempAccount.positionValue = myAccount.positionValue;
                tempAccount.totalAssets = myAccount.totalAssets;
                tempAccount.initialAssets = myAccount.initialAssets;
                accountHistory.Add(tempAccount);

                //账户信息更新
                AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, minuteData[0].time, dataToday);
            }

            //策略绩效统计及输出
            PerformanceStatisics myStgStats = new PerformanceStatisics();
            myStgStats = PerformanceStatisicsUtils.compute(accountHistory, positions);
            //画图
            Dictionary<string, double[]> line = new Dictionary<string, double[]>();
            double[] netWorth = accountHistory.Select(a => a.totalAssets / initialCapital).ToArray();
            line.Add("NetWorth", netWorth);
            //记录净值数据
            RecordUtil.recordToCsv(accountHistory, GetType().FullName, "account", parameters: "EMA7_EMA50", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录持仓变化
            //var positionStatus = OptionRecordUtil.Transfer(positions);
            //RecordUtil.recordToCsv(positionStatus, GetType().FullName, "positions", parameters: "EMA7_EMA50", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //记录统计指标
            var performanceList = new List<PerformanceStatisics>();
            performanceList.Add(myStgStats);
            RecordUtil.recordToCsv(performanceList, GetType().FullName, "performance", parameters: "EMA7_EMA50", performance: myStgStats.anualSharpe.ToString("N").Replace(".", "_"));
            //统计指标在console 上输出
            Console.WriteLine("--------Strategy Performance Statistics--------\n");
            Console.WriteLine(" netProfit:{0,5:F4} \n totalReturn:{1,-5:F4} \n anualReturn:{2,-5:F4} \n anualSharpe :{3,-5:F4} \n winningRate:{4,-5:F4} \n PnLRatio:{5,-5:F4} \n maxDrawDown:{6,-5:F4} \n maxProfitRatio:{7,-5:F4} \n informationRatio:{8,-5:F4} \n alpha:{9,-5:F4} \n beta:{10,-5:F4} \n averageHoldingRate:{11,-5:F4} \n", myStgStats.netProfit, myStgStats.totalReturn, myStgStats.anualReturn, myStgStats.anualSharpe, myStgStats.winningRate, myStgStats.PnLRatio, myStgStats.maxDrawDown, myStgStats.maxProfitRatio, myStgStats.informationRatio, myStgStats.alpha, myStgStats.beta, myStgStats.averageHoldingRate);
            Console.WriteLine("-----------------------------------------------\n");

            //benchmark净值
            //List<double> netWorthOfBenchmark = benchmark.Select(x => x / benchmark[0]).ToList();
            //line.Add("Base", netWorthOfBenchmark.ToArray());
            string[] datestr = accountHistory.Select(a => a.time.ToString("yyyyMMdd")).ToArray();
            //初始化净值曲线类
            PLChart plc = new PLChart(line, datestr);
            Application.Run(plc);

            //【注意】PLChart的SaveZed函数已经进行了修改，使用前请确定参数正确
            //plc.SaveZed("D:\\BTP\\Result\\BackTestingPlatform.Strategies.Stock.StockSample.MABreak\\aa.png");

            return 0;
        }
    }
}
