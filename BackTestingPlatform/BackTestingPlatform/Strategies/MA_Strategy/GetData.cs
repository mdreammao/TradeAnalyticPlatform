using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;


namespace BackTestingPlatform.Strategies.MA_Strategy
{
    class GetData
    {
        public void Execute()
        {
            //从万得提取数据
            WindAPI w = new WindAPI();
            w.start();
            WindData wd = w.wsi("510050.SH", "open,high,low,close,volume"
                , "2016-07-01 09:00:00", "2016-07-22 15:00:00", "Fill=Previous");                          
            w.stop();
            //初始化各类变量
            DateTime[] DataDate = wd.timeList;
            String.Format("yyyy/MM/dd hh:mm:ss", DataDate);
            //日期、开盘价、最高价、最低价、收盘价、成交量
            int FieldLength = wd.GetFieldLength();
            int DataLength = wd.GetDataLength();
            int DateLength = DataLength / FieldLength;
            int sign = 0;//下标计数
            double[] RawData = (double[])wd.data;
            double[,] MarketData = new double[DateLength,5];
            /*
            double[] OP = new double[DateLength];
            double[] HP = new double[DateLength];
            double[] LP = new double[DateLength];
            double[] CP = new double[DateLength];
            */
            for (int i = 0; i < RawData.Length; i = i + FieldLength)
            {
                for(int  j= 0; j<FieldLength; j++)
                    MarketData[sign,j] = RawData[i+j];
                sign++;  
            }
        //    System.Console.WriteLine("{0},{1},{2},{3}", sign, MarketData.GetLength(0), MarketData.GetLength(1), DateLength);           
            /*
            for (int now = 0; now <= sign; now++)
            {
                if (now != 0 && now % 60 == 0)//60min做一次停顿
                    Console.ReadKey();
                System.Console.WriteLine("Time:{0} -- O:{1} H:{2} L:{3} C:{4} Volum:{5}\n"
                    , DataDate[now], MarketData[now,0], MarketData[now,1]
                    , MarketData[now,2], MarketData[now,3], MarketData[now,4]);
            }
            Console.ReadKey();   
             */
            GenSignal myGS = new GenSignal();
            myGS.getSignal(MarketData);
        }
        
    }
}
