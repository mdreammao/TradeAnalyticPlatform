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
using BackTestingPlatform.Model.Position;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Strategies;


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
            List<DateTime> timelist = (List<DateTime>)Platforms.BasicInfo["TradeDays"];
           
         
            DateTime startDate = new DateTime(2016, 7, 1);//策略起止时间
            DateTime endDate = new DateTime(2016, 8, 1); ;
            DateTime nowDate;//当前时间，每次循环传入策略，返回开仓或平仓信息
            
            DateTime d1=(DateTime)TradeDaysUtils.RecentTradeDay(startDate);
            DateTime d2=(DateTime)TradeDaysUtils.RecentTradeDay(endDate);
            int indexOfStart=timelist.BinarySearch(d1);
            int indexOfEnd=timelist.BinarySearch(d2);
           

            //初始化账户信息
            AccountInfo account1 = new AccountInfo();
            List<AccountHistory> account1List = new List<AccountHistory>();//存放该账户的历史交易记录
            account1.positionStatus = 0;//初始持仓状态为0
            account1.AccountID = 1;//账户ID

            double[,] signalArray = new double[timelist.Count,3];//记录交易信号，暂时存放在二维数组

            SingleMA sma = new SingleMA();
            DealJudge myDeal = new DealJudge();
        //    int canDeal;//成交判断
            double[] transReturn;//成交回报
            for (int tic = 0; tic < timelist.Count; tic++)
            {
                nowDate = timelist[tic];
                double[] tempArray =  sma.stg(startDate, nowDate, account1);
                signalArray[tic, 1] = tempArray[1];//是否成交，0/1
                signalArray[tic, 2] = tempArray[2];//交易价格
                signalArray[tic, 3] = tempArray[3];//交易量，单位：手
                if (signalArray[tic, 1] != 0)
                    transReturn = myDeal.Judge(nowDate, tempArray);
                
                Console.WriteLine("Time:{0}  Signal:{1}", nowDate,tempArray[1]);
             //   
                
            }                                    
        }
    }
}
