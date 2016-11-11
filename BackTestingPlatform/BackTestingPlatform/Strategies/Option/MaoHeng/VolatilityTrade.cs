using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.TALibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Strategies.Option.MaoHeng
{
    public class VolatilityTrade
    {
        private DateTime startDate, endDate;
        private List<StockDaily> etfDailyData;
        private int startIndex;
        private int step;
        private int backTestingDuration;
        private double[] etfVol;
        public VolatilityTrade(int startDate,int endDate,int step=20)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            backTestingDuration= DateUtils.GetSpanOfTradeDays(this.startDate, this.endDate);
            this.step = step;
            etfDailyData = getETFHistoricalDailyData();
            startIndex=etfDailyData.Count()- backTestingDuration;
        }
        public void compute()
        {
            //统计历史波动率分位数,从回测期开始前一天，统计到最后一天
            double[][] fractile = new double[backTestingDuration+1][];
            fractile = computeFractile(startIndex-1,etfDailyData.Count()-1);
            //按时间遍历，从2015年02月09日50ETF期权上市开始
            for (int i = startIndex; i <startIndex+ backTestingDuration; i++)
            {
                double fractile70Yesterday = fractile[i - 1][7];
                double volYesterday = etfVol[i - 1];
            }
        }

        private List<StockDaily> getETFHistoricalDailyData()
        {
            return Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050.SH", Kit.ToDate(20130101), endDate);
        }

        /// <summary>
        /// 计算历史波动率的分位数
        /// </summary>
        /// <returns></returns>
        private double[][] computeFractile(int start,int end)
        {

            double[][] disArr = new double[etfDailyData.Count()][];
            //获取前复权的价格
            double[] etfPrice=new double[etfDailyData.Count()];
            for (int i = 0; i < etfDailyData.Count(); i++)
            {
                etfPrice[i] = etfDailyData[i].close * etfDailyData[i].adjustFactor / etfDailyData.Last().adjustFactor;
            }
            //获取ETF每日年化波动率
            double[] etfVol = new double[etfDailyData.Count()];
            etfVol = Volatility.HVYearly(etfPrice, step);
            this.etfVol = etfVol;
            //统计每日波动率分位数
            List<double> volList = new List<double>();
            for (int i = 1; i < etfPrice.Count(); i++)
            {
                //按循序依次向数组中插入波动率
                if (volList.Count()==0)
                {
                    volList.Add(etfVol[i]);
                }
                else 
                {
                    if (etfVol[i]<volList[0])
                    {
                        volList.Insert(0, etfVol[i]);
                    }
                    else if (etfVol[i]>volList.Last())
                    {
                        volList.Insert(volList.Count(), etfVol[i]);
                    }
                    else
                    {
                        for (int j = 1; j < volList.Count()-1; j++)
                        {
                            if (etfVol[i]>volList[j-1] && etfVol[i]<=volList[j])
                            {
                                volList.Insert(j, etfVol[i]);
                                continue;
                            }
                        }
                    }
                }
                if (i>=start)
                {
                    int L = volList.Count() - 1;
                    disArr[i] = new double[11];
                    disArr[i][0] = volList[0];
                    disArr[i][1] = volList[(int)Math.Ceiling(L * 0.1)];
                    disArr[i][2] = volList[(int)Math.Ceiling(L * 0.2)];
                    disArr[i][3] = volList[(int)Math.Ceiling(L * 0.3)];
                    disArr[i][4] = volList[(int)Math.Ceiling(L * 0.4)];
                    disArr[i][5] = volList[(int)Math.Ceiling(L * 0.5)];
                    disArr[i][6] = volList[(int)Math.Ceiling(L * 0.6)];
                    disArr[i][7] = volList[(int)Math.Ceiling(L * 0.7)];
                    disArr[i][8] = volList[(int)Math.Ceiling(L * 0.8)];
                    disArr[i][9] = volList[(int)Math.Ceiling(L * 0.9)];
                    disArr[i][10] = volList[L];
                }
            }
            return disArr;

        }

    }
}
