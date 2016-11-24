using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Model.Positions;
using BackTestingPlatform.Model.Common;
using BackTestingPlatform.DataAccess.Stock;
using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.Utilities.TimeList;
using BackTestingPlatform.Model.Signal;
using System.Configuration;
using BackTestingPlatform.Transaction.MinuteTransactionWithSlip;
using BackTestingPlatform.AccountOperator.Minute;
using BackTestingPlatform.Utilities.Common;
using BackTestingPlatform.Charts;
using BackTestingPlatform.Utilities.SaveResult.Common;
using System.Windows.Forms;


namespace BackTestingPlatform.Strategies.Stock.StockSample
{
    //当前两个任务：
    //1. 使用回测平台做出15K交易程序
    //2. 为该平台添加模拟交易模块，可以通过模拟账户交易股票

    //还需要做的
    //1. 目前是全部加仓和全部减仓，未来要设计成部分加仓，全部减仓

    public class MeanReverting15K
    {
        //构造函数中需要传入回测的起始时间和需要进行回测的标的代码
        private DateTime startDate, endDate;
        private string stockCode = "";

        //记录数据为分钟数据，现将周期调整至15分钟级别，在策略中最基础数据从0开始，第一个15K为0-14
        //private int cycle = 14;

        //爬山标记，标记是在寻找最大值还是最小值，默认为寻找最大值
        private int climb = 1;

        //回测参数设置，initialCapital为账户初始资本金，slipPoint为每笔成交的滑移价差比率
        private double initialCapital = 10000000;
        private double slipPoint = 0.005;

        //记录当前持仓数量
        private double volumeNow = 0;

        //初始化log工具
        static Logger log = LogManager.GetCurrentClassLogger();

        public MeanReverting15K(int start, int end, string code)
        {
            startDate = Kit.ToDate(start);
            endDate = Kit.ToDate(end);
            stockCode = code;
        }

        /// <summary>
        /// 50ETF择时策略测试，N-Days Reversion
        /// </summary>
        public void compute()
        {
            log.Info("开始回测(回测期{0}到{1})", Kit.ToInt_yyyyMMdd(startDate), Kit.ToInt_yyyyMMdd(endDate));

            //将来可以把这些初始化操作从程序中分离，写在外面
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

            ///数据准备
            //交易日信息
            List<DateTime> tradeDays = DateUtils.GetTradeDays(startDate, endDate);
            //分钟数据准备，取全回测期的数据存放于data
            Dictionary<string, List<KLine>> data = new Dictionary<string, List<KLine>>();
            foreach (var tempDay in tradeDays)
            {
                var stockData = Platforms.container.Resolve<StockMinuteRepository>().fetchFromLocalCsvOrWindAndSave(stockCode, tempDay);
                if (!data.ContainsKey(stockCode))
                    data.Add(stockCode, stockData.Cast<KLine>().ToList());
                else
                    data[stockCode].AddRange(stockData.Cast<KLine>().ToList());
            }

            //交易开关设置，控制day级的交易开关，tradingOn还没有使用
            bool tradingOn = true;//总交易开关
            bool openingOn = true;//开仓开关
            bool closingOn = false;//平仓开关

            //定义交易信号数组
            Dictionary<string, MinuteSignal> signal = new Dictionary<string, MinuteSignal>();

            ///回测循环
            //回测循环--By Day
            foreach (var day in tradeDays)
            {
                //取出当天的数据,列表类型，包含high,low,amt等数据
                //取出当天的数据
                Dictionary<string, List<KLine>> dataToday = new Dictionary<string, List<KLine>>();
                foreach (var variety in data)
                {
                    dataToday.Add(variety.Key, data[variety.Key].FindAll(s => s.time.Year == day.Year && s.time.Month == day.Month && s.time.Day == day.Day));
                }

                //是否为回测最后一天
                bool isLastDayOfBackTesting = day.Equals(endDate);

                //先测试15K数据，直接将策略写在程序中，将来可以尝试分离过程，将部分策略以函数或者类的形式写在外面
                //现将1分钟数据调整为15分钟数据，15K数据占用16个数组空间
                List<KLine> data15K = Get15KData(dataToday[stockCode]);
                //交易信号判断，用信号来判断开仓还是平仓，但是在交易单元，还要靠平均价和当前价，进行止损平仓
                List<int> tradeSignal = ClimbMountain(data15K);

                //回测循环 -- By Minute
                //不允许在同一根1minBar上开平仓
                int index = 0;
                while (index < 16)
                {
                    int nextIndex = index + 1;
                    if (nextIndex == 16)
                        break;
                    DateTime now = data15K[index].time;
                    DateTime next = data15K[nextIndex].time;

                    if((tradeSignal[index] == 0) && openingOn)
                    {
                        //设置signal信号，设置时间等参数
                        MinuteSignal openSignal = new MinuteSignal()
                        {
                            code = stockCode,
                            //开仓只开90%，下一阶段可以分批加仓，全部减仓
                            volume = myAccount.freeCash / data15K[nextIndex].open * 0.9,
                            time = data15K[nextIndex].time,
                            tradingVarieties = "stock",
                            price = data15K[nextIndex].open,
                            minuteIndex = nextIndex
                        };
                        openingOn = false;
                        closingOn = true;
                        volumeNow = myAccount.freeCash / data15K[nextIndex].open * 0.9;
                        signal.Add(stockCode, openSignal);

                        //开仓下单
                        MinuteTransactionWithSlip.computeMinuteOpenPositions(signal, dataToday, ref positions,
                            ref myAccount, slipPoint: slipPoint, now: data15K[nextIndex].time, capitalVerification: false);
                        signal.Clear();
                    }
                    else if(((tradeSignal[index] == 1)||(positions[now][stockCode].ShortPosition.averagePrice < data15K[index].close)) 
                        && (volumeNow>0) && closingOn)
                    {
                        //设置signal信号，设置时间等参数
                        MinuteSignal closeSignal = new MinuteSignal()
                        {
                            code = stockCode,
                            volume = 0 - volumeNow,
                            time = data15K[nextIndex].time,
                            tradingVarieties = "stock",
                            price = data15K[nextIndex].open,
                            minuteIndex = 0
                        };
                        openingOn = true;
                        closingOn = false;
                        volumeNow = 0;
                        signal.Add(stockCode, closeSignal);

                        //平仓下单
                        MinuteTransactionWithSlip.computeMinuteClosePositions(signal, dataToday, ref positions,
                            ref myAccount, slipPoint: slipPoint, now: data15K[nextIndex].time);
                        signal.Clear();
                    }

                    index = nextIndex;
                }
                //账户信息记录By Day            
                //用于记录的临时账户
                BasicAccount tempAccount = new BasicAccount();
                tempAccount.time = myAccount.time;
                tempAccount.freeCash = myAccount.freeCash;
                tempAccount.margin = myAccount.margin;
                tempAccount.positionValue = myAccount.positionValue;
                tempAccount.totalAssets = myAccount.totalAssets;
                accountHistory.Add(tempAccount);

                //账户信息更新
                AccountUpdatingForMinute.computeAccountUpdating(ref myAccount, positions, myAccount.time, dataToday);
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
            plc.SaveZed("D:\\BTP\\Result\\BackTestingPlatform.Strategies.Stock.StockSample.MABreak\\aa.png");
        }

        //爬山策略,15K数据有16个周期，这里粗暴一些，直接写死在程序中了
        private List<int> ClimbMountain(List<KLine> todayData)
        {
            if (todayData == null)
            {
                log.Info("策略函数中，15K数据为空");
                return null;
            }
            //设定价格信号和成交量信号，当二者满足对应关系，才执行操作
            int priceSignal = -1, volumeSignal = -1;
            int priceClimb = 1, volumeClimb = 1;
            double priceLow = 0, priceCurrent = 0, priceHigh = 0, volumeLow = 0, volumeCurrent = 0, volumeHigh = 0;
            List<int> signal = new List<int>();
            for (int i = 0; i < todayData.Count(); ++i)
            {
                //最大值需要在策略开始时，给最大值赋个值，最小值不用管
                if (i == 0)
                {
                    priceHigh = todayData[0].high;
                    volumeHigh = todayData[0].amount;
                }
                //价格爬山,爬山时寻找最大值，下山时寻找最小值
                if (priceClimb == 1)
                    priceCurrent = todayData[i].high;
                else
                    priceCurrent = todayData[i].low;

                if (priceClimb == 1)
                {
                    if (priceCurrent > priceHigh)
                        priceHigh = priceCurrent;
                    else
                    {
                        priceLow = todayData[i].low;
                        priceClimb = 0;
                        priceSignal = 1;
                    }
                }
                else
                {
                    if (priceCurrent < priceLow)
                        priceLow = priceCurrent;
                    else
                    {
                        priceHigh = todayData[i].high;
                        priceClimb = 1;
                        priceSignal = 0;
                    }
                }

                //成交量爬山,volumeCurrent不需要进行判断，因为成交量在一个15K不存在最大最小值
                volumeCurrent = todayData[i].amount;
                if (volumeClimb == 1)
                {
                    if (volumeCurrent > volumeHigh)
                        volumeHigh = volumeCurrent;
                    else
                    {
                        volumeLow = todayData[i].low;
                        volumeClimb = 0;
                        volumeSignal = 1;
                    }
                }
                else
                {
                    if (volumeCurrent < volumeLow)
                        volumeLow = volumeCurrent;
                    else
                    {
                        volumeHigh = todayData[i].high;
                        volumeClimb = 1;
                        volumeSignal = 0;
                    }
                }
                //是否开仓平仓判断,平仓信号，1平仓，0开仓
                if (priceSignal == 1 && volumeSignal == 0)
                    signal.Add(1);
                else if (priceSignal == 0 && volumeSignal == 1)
                    signal.Add(0);
                else
                    signal.Add(-1);
            }
            signal.Reverse();
            return signal;
        }

        //从1分钟数据中提取出15K数据，如果取数成功返回整理过后的list文件否则返回空
        private List<KLine> Get15KData(List<KLine> todayData)
        {
            //每当index/15=0时进行写数据，记录第1分钟的开盘，最后1分钟的收盘和时间，总成交量、额，持仓
            int index = 0;
            double volume = 0, amount = 0;
            List<KLine> newData = new List<KLine>(new KLine[16]);
            for (int i = 0; i < 16; ++i)
                newData[i] = new KLine();

            if (todayData == null)
            {
                log.Info("Get15KData函数传入参数，data为空！返回null");
                return null;
            }

            //每次需要加入的15K数据
            //KLine data15K = new KLine();
            for (index = 0; index < 240; ++index)
            {
                //对每个15K的成交量、额数据进行累加
                volume += todayData[index].volume;
                amount += todayData[index].amount;

                if (index % 15 == 0 || index == 239)
                {
                    if (index == 0)
                    {
                        newData[0].open = todayData[index].open;
                        newData[0].high = todayData[index].high;
                        newData[0].low = todayData[index].low;
                    }
                    else
                    {
                        //注意这里取不到成交量最大值和最小值，需要在策略程序中写出这个功能
                        //先给这个
                        newData[index / 15 - 1].time = todayData[index - 1].time;
                        newData[index / 15 - 1].close = todayData[index - 1].close;
                        newData[index / 15 - 1].high = GetHigh(todayData, index - 15, index - 1);
                        newData[index / 15 - 1].low = GetLow(todayData, index - 15, index - 1);
                        newData[index / 15 - 1].volume = volume;
                        newData[index / 15 - 1].amount = amount;
                        newData[index / 15 - 1].openInterest = todayData[index].openInterest;
                        //newData.Add(data15K);

                        //重新给data15K更新值
                        newData[index / 15].open = todayData[index].open;
                        newData[index / 15].high = todayData[index].high;
                    }
                    if (index != 0)
                    {
                        volume = 0; amount = 0;
                    }
                }
            }

            return newData;
        }

        //找出分钟数据中，价格的最大值，-1为空出错，正值为找到的最大值
        private double GetHigh(List<KLine> todayData, int index, int nextIndex)
        {
            if (todayData == null)
            {
                log.Info("getHigh函数传入参数，data为空！");
                return -1;
            }
            double highPrice = todayData[index].high;
            for (int i = index + 1; i <= nextIndex; ++i)
                if (todayData[i].high > highPrice)
                    highPrice = todayData[i].high;
            return highPrice;
        }
        //找出分钟数据中，价格最小值，-1为空出错，正值为找到的最小值
        private double GetLow(List<KLine> todayData, int index, int nextIndex)
        {
            if (todayData == null)
            {
                log.Info("getHigh函数传入参数，data为空！");
                return -1;
            }
            //找价格最大值
            double lowPrice = todayData[index].low;
            for (int i = index + 1; i <= nextIndex; ++i)
                if (todayData[i].low < lowPrice)
                    lowPrice = todayData[i].low;
            return lowPrice;
        }
    }
}
