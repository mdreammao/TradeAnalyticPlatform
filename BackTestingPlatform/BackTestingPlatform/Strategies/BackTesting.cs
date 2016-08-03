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
using System.String;



namespace BackTestingPlatform.Strategies
{
    class BackTesting :Strategy
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
            string path = @"C:\Users\HFY\Source\Repos\TradeAnalyticPlatform2\BackTestingPlatform\BackTestingPlatform\RESOURCES\trade_days_2010_2016.txt";
            string[] lines = File.ReadAllLines(path);
          
            int[] signalArray =new int[lines.Length];//记录交易信号，暂时存放在一维数组

            DateTime startDate = new DateTime(2016, 7, 1);//策略起止时间
            DateTime endDate = new DateTime(2016, 8, 1); ;
            DateTime nowDate;//当前时间，每次循环传入策略，返回开仓或平仓信息
            List<DateTime> timelist =new List<DateTime>();//存放起止日之间的tradedays
            
            //从TradDays里获取startDate至endDate的list
            int indexOfStart = lines.ToList().IndexOf(startDate.ToString("yyyyMMdd"));
            int indexOfEnd = lines.ToList().IndexOf(startDate.ToString("yyyyMMdd"));
            for (int i = indexOfStart,j=0; i < indexOfEnd +1 ; i++,j++)
            {
                timelist[j] = Convert.ToDateTime(lines[i]);
            }

            //初始化账户信息
            AccountInfo account1 = new AccountInfo();
            List<AccountHistory> account1List = new List<AccountHistory>();//存放该账户的历史交易记录
            account1.positionStatus = 0;//初始持仓状态为0
            account1.AccountID = 1;//账户ID
                  
            SingleMA sma = new SingleMA();
            DealJudge myDeal = new DealJudge();
        //    int canDeal;//成交判断
            double[] TransReturn;//成交回报
            for (int tic = 0; tic < timelist.Count; tic++)
            {
                nowDate = timelist[tic];
                signalArray[tic] = sma.stg(startDate, nowDate, account1);
                TransReturn = myDeal.TransactionReturn(nowDate,);
                
            }                                    
        }
    }
}
