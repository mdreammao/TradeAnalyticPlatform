using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Utilities;
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
        private class distribution
        {
            public double[] fractile = new double[11];
        }
        public VolatilityTrade(int startDate,int endDate)
        {
            this.startDate = Kit.ToDateTime(startDate);
            this.endDate = Kit.ToDateTime(endDate);
            etfDailyData = getETFHistoricalDailyData();
        }
        public void compute()
        {
            //统计历史波动率分位数
            distribution[] fractile = computeFractile();
        }

        private List<StockDaily> getETFHistoricalDailyData()
        {
            return Platforms.container.Resolve<StockDailyRepository>().fetchFromLocalCsvOrWindAndSave("510050SH", Kit.ToDateTime(20070104), endDate);
        }

        /// <summary>
        /// 计算历史波动率的分位数
        /// </summary>
        /// <returns></returns>
        private distribution[] computeFractile()
        {
            int length= DateUtils.GetSpanOfTradeDays(startDate,endDate);
            distribution[] disArr = new distribution[length];
            //获取前复权的价格
            double[] etfPrice=new double[etfDailyData.Count()];
            for (int i = 0; i < etfDailyData.Count(); i++)
            {
                etfPrice[i] = etfDailyData[i].close * etfDailyData[i].adjustFactor / etfDailyData.Last().adjustFactor;
            }
            //获取ETF每日年化波动率
            double[] etfVol = new double[etfDailyData.Count()];
            for (int i = 1; i < etfPrice.Length; i++)
            {
                etfVol[i] = Math.Log(etfPrice[i] / etfPrice[i - 1]) * Math.Sqrt(252);
            }
            //统计每日波动率分位数
            List<double> volList = new List<double>();
            for (int i = 1; i < length; i++)
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
                if (i>=100)
                {
                    int L = volList.Count() - 1;
                    disArr[i].fractile[0] = volList[0];
                    disArr[i].fractile[1] = volList[(int)Math.Ceiling(L * 0.1)];
                    disArr[i].fractile[2] = volList[(int)Math.Ceiling(L * 0.2)];
                    disArr[i].fractile[3] = volList[(int)Math.Ceiling(L * 0.3)];
                    disArr[i].fractile[4] = volList[(int)Math.Ceiling(L * 0.4)];
                    disArr[i].fractile[5] = volList[(int)Math.Ceiling(L * 0.5)];
                    disArr[i].fractile[6] = volList[(int)Math.Ceiling(L * 0.6)];
                    disArr[i].fractile[7] = volList[(int)Math.Ceiling(L * 0.7)];
                    disArr[i].fractile[8] = volList[(int)Math.Ceiling(L * 0.8)];
                    disArr[i].fractile[9] = volList[(int)Math.Ceiling(L * 0.9)];
                    disArr[i].fractile[10] = volList[L];
                }
            }
            return disArr;

        }

    }
}
