using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Core;
using BackTestingPlatform.Strategies.MA;
using System.IO;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Strategies;
using BackTestingPlatform.DataAccess;
using Autofac;
using WAPIWrapperCSharp;
using BackTestingPlatform.Model.Positions;

namespace BackTestingPlatform.Strategies
{
    public class BackTesting : Strategy
    {
        /// <summary>
        /// 1.该类将从回测startDate到endDate，按TradeDays循环向策略传递nowDate，以保证策略内可引用时间不会超过nowDate，
        /// 避免data snooping bias
        /// 2.
        /// </summary>
        /// 
        //   void act();
        public void act()
        {
            stgBooter();
        }

        //执行日期循环动作，从startDate到endDate，按TradeDays循环向策略传递nowDate
        public void stgBooter()
        {

            List<DateTime> timeList = Caches.get<List<DateTime>>("TradeDays");
            int None = Constants.NONE;//空值

            DateTime startDate = new DateTime(2016, 6, 1);//策略起止时间
            DateTime endDate = new DateTime(2016, 7, 1); ;
            DateTime nowDate;//当前时间，每次循环传入策略，返回开仓或平仓信息

            DateTime d1 = (DateTime)TradeDaysUtils.NextOrCurrent(startDate);
            DateTime d2 = (DateTime)TradeDaysUtils.NextOrCurrent(endDate);
            int indexOfStart = timeList.BinarySearch(d1);
            int indexOfEnd = timeList.BinarySearch(d2);

            //真实起止日期
            startDate = timeList[indexOfStart];
            endDate = timeList[indexOfEnd];

            //取得在回测周期的barsList,存放每个bar的时间(timeList为日频，转化为所需频率的时间列表)
            //意义在于，每次回测，策略可使用当前bar及之前任意的数据，数据库自取

            WindAPI wapi = Platforms.GetWindAPI();
            // WindData d = wapi.wsi("510050.SH", "open,high,low,close,volume,amt", startDate.ToString(), endDate.ToString(), "");
            WindData d = wapi.wsi("510050.SH", "open,high,low,close,volume,amt", startDate, endDate, "");
            List<DateTime> barsList = new List<DateTime>();
            barsList = new List<DateTime>(d.timeList);

            //初始化账户信息
            AccountInfo account1 = new AccountInfo();
            List<AccountHistory> account1List = new List<AccountHistory>();//存放该账户的历史交易记录
            account1.positionStatus = 0;//初始持仓状态为0
            account1.AccountID = 1;//账户ID


            //   Console.WriteLine("Here!");

            double[,] signalArray = new double[barsList.Count, 3];//记录交易信号，暂时存放在二维数组

            SingleMA sma = new SingleMA();
            DealJudge myDeal = new DealJudge();
            double[] transReturn = new double[4];//成交回报
            //成交回报初始化
            transReturn[0] = 0;
            transReturn[1] = 1;
            transReturn[2] = None;
            transReturn[3] = 1;


            //回测循环
            for (int tic = 1; tic < barsList.Count; tic++)
            {
                nowDate = barsList[tic];
                double[] tempArray = sma.stg(startDate, nowDate, account1);
                signalArray[tic, 0] = tempArray[0];//是否成交，0/1/-1
                signalArray[tic, 1] = tempArray[1];//交易价格
                signalArray[tic, 2] = tempArray[2];//交易量，单位：手

                if (signalArray[tic, 1] != 0)
                    transReturn = myDeal.Judge(nowDate, tempArray);

                //更新账户信息
                if (signalArray[tic, 0] == 1 & transReturn[0] == 1)//信号开多且可成交
                {
                    account1.positionStatus = 1;
                    account1.lastBuyPrice = account1.lastBuyPrice = transReturn[2];//记录买入价          
                }

                else if (signalArray[tic, 0] == -1 & transReturn[0] == 1)//信号平多且可成交
                {
                    account1.positionStatus = 0;
                    account1.netWorth = account1.netWorth * (1 + (transReturn[2] - account1.lastBuyPrice) / account1.lastBuyPrice);//净值累积
                    account1.lastBuyPrice = 0;
                }
                else if (signalArray[tic, 0] == 0 & account1.positionStatus == 1)//无信号有仓位时净值随行情变化
                    account1.netWorth = account1.netWorth * (1 + (tempArray[1] - account1.lastBuyPrice) / account1.lastBuyPrice);//净值累积

                //记录历史交易
                account1List.Add(new AccountHistory
                {
                    tradeTime = nowDate,
                    netWorth = account1.netWorth
                });

                Console.WriteLine("Time:{0}  Signal:{1}  Price:{2}  Position:{3} NetWorth:{4,8:F3}\n",
                    nowDate, tempArray[0], tempArray[1], account1.positionStatus, account1.netWorth);
                Console.WriteLine("-------------------------------");
            }

            for (int temp = 0; temp < account1List.Count; temp++)
            {
                Console.WriteLine("-----------Histroy NetWorth--------------------");
                Console.WriteLine("Time:{0,-12}  NetWorth:{1}",account1List[temp].tradeTime,account1List[temp].netWorth);
                if (temp % 49 == 0)
                    Console.ReadKey();
                  
            }

        }
    }
}

