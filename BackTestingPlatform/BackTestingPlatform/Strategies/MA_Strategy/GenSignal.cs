using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.MA_Strategy
{
    class GenSignal
    {       
        //计算指标，test：N日均线，存放在二维数组RelatedIndex
        public int[] getSignal(double[,] MarketData)
        {
            int rowLen=MarketData.GetLength(0);
            int columnLen=MarketData.GetLength(1);
            int indexNum = 2;
            double[,] Index = new double[rowLen, indexNum];//存放自定义指标
            int[] SignalMatrix = new int[rowLen];//存放交易信号         
            Index = CalcIndex(MarketData);
            TradingDecision(MarketData,Index, ref SignalMatrix);
            return SignalMatrix;
        
        }

        private double[,] CalcIndex(double[,] MarketData)
        {           
            //MarketData数据结构为N行M列，N为时间点数，M为源数据数，一般为O、H、L、C、Volum
            int rowLen=MarketData.GetLength(0);
            int columnLen=MarketData.GetLength(1);
            int indexNum = 1;
            double[,] IndexMatrix = new double[rowLen, indexNum];
            //计算均线指标值，存入IndexMatrix中，以5日均线为例,**单参数计算**
            int param1 = 20;
        //  int param2 = 10;
            //取出收盘价
            double[] tempMarketData = new double[rowLen];
            for (int num = 0; num < rowLen; num++)
                tempMarketData[num] = MarketData[num, 3];//取出收盘价
            for (int i = 0; i < rowLen; i ++ )
            {
                //List <int> tempList =new List<int> (); //存放临时的行情数据
                double[] tempList1 = new double[param1];
             // double[] tempList2 = new double[param2];                
                
                //各时点指标计算
                if (i < param1 - 1)//起始点为0，故实际天数减1
                    IndexMatrix[i, 0] = -9999;
                else
                {
                    Array.Copy(tempMarketData, i - param1 + 1, tempList1, 0, param1);//每次将所需数据放入tempList1
                    IndexMatrix[i, 0] = tempList1.Average();  //计算N日简单平均 
                }
                                            
            }
            return IndexMatrix;
            //测试输出
            /*
            for(int now = 0; now < rowLen; now++)
                System.Console.WriteLine("{0}-Bars SMA is：{1}",param1,IndexMatrix[now,0]);
            Console.ReadKey();
            return IndexMatrix;
            */
        }

        private void TradingDecision(double[,]MarketData, double[,] Index,ref int[] SignalMatrix)
        {
            int rowLen = Index.GetLength(0);
            int indexNum = Index.GetLength(1);
            int PositonStatus = 0;//仓位状态，初始为0，视为不允许卖空
            //取出收盘价
            double[] tempMarketData = new double[rowLen];
            for (int num = 0; num < rowLen; num++)
                tempMarketData[num] = MarketData[num, 3];
           
            for(int bar = 1;bar <rowLen; bar++)//需要判断上一根bar，从1开始
            { 
                //InitialStatus
                if(bar==1)
                    SignalMatrix[0] = 0;
                //CrossUp
                if (tempMarketData[bar] >= Index[bar, 0] && tempMarketData[bar - 1] < Index[bar - 1, 0]
                    && PositonStatus==0)
                {
                    SignalMatrix[bar] = 1;
                    PositonStatus = 1;
                }                   
                //CrossDown
                else if(tempMarketData[bar] <= Index[bar, 0] && tempMarketData[bar - 1] > Index[bar - 1, 0]
                    && PositonStatus==1)
                {
                    SignalMatrix[bar] = -1;
                    PositonStatus = 0;
                }
                      
                else
                    SignalMatrix[bar] = 0;
            }
            for (int now = 0; now < rowLen; now++)
                System.Console.WriteLine("Now Signal is：{0}--CP:{1} MA:{2}"
                    , SignalMatrix[now], tempMarketData[now], Index[now, 0]);
            Console.ReadKey();
       
        }

    }

}