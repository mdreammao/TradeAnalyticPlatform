using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model;

namespace BackTestingPlatform.Model.TALibrary
{
    class TA_MA
    {
        private int None=Constants.NONE; //空值
      //  public double[] stockPrice; //价格序列
        /**/
        public TA_MA(double[] stockPrice)
        {
            // TODO: Complete member initialization
            this.stockPrice = stockPrice;
        }
        
        //    public int weight = 1;//MA的权重

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MAParam">股票代码</param>

        public double[] SMA(int MAParam)
        {          
            double[] retData = new double[stockPrice.Length];//存放计算结果，用于返回
            double[] tempList = new double[MAParam]; 

            for (int i = 0; i < stockPrice.Length; i++)
            {          
                                          
                //各时点指标计算
                if (i < MAParam - 1)//起始点为0，故实际天数减1
                    retData[i] = None;
                else
                {
                    Array.Copy(stockPrice, i - MAParam + 1, tempList, 0, MAParam);//每次将所需数据放入tempList1
                    retData[i] = tempList.Average();  //计算N日简单平均 
                }

            }
            return retData;
        }


        public double[] stockPrice { get; set; }
    }
}
