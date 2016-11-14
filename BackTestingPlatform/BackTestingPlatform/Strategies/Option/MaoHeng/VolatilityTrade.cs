using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
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
        private double[] optionVol;
        private List<OptionInfo> optionInfoList;
        public VolatilityTrade(int startDate,int endDate,int step=20)
        {
            this.startDate = Kit.ToDate(startDate);
            this.endDate = Kit.ToDate(endDate);
            backTestingDuration= DateUtils.GetSpanOfTradeDays(this.startDate, this.endDate);
            this.step = step;
            etfDailyData = getETFHistoricalDailyData();
            startIndex=etfDailyData.Count()- backTestingDuration;
            optionInfoList = Platforms.container.Resolve<OptionInfoRepository>().fetchFromLocalCsvOrWindAndSaveAndCache(1);
        }
        public void compute()
        {
            //统计历史波动率分位数,从回测期开始前一天，统计到最后一天
            double[][] fractile = new double[backTestingDuration+1][];
            fractile = computeFractile(startIndex-1,etfDailyData.Count()-1);
            //统计隐含波动率
            computeImpv();
            //按时间遍历，从2015年02月09日50ETF期权上市开始
            for (int i = startIndex; i <startIndex+ backTestingDuration; i++)
            {
                double fractile70Yesterday = fractile[i - 1][7];
                double volYesterday = etfVol[i - 1];
            }
        }

        private void computeImpv()
        {
            optionVol = new double[etfDailyData.Count()];
            for (int i = startIndex; i < etfDailyData.Count(); i++)
            {
                DateTime today = etfDailyData[i].time;
                double etfPrice = etfDailyData[i].close;
                double volThisMonth;
                double volNextMonth;
                double duration;
                //获取当日期限结构,选取当月合约
                List<double> dateStructure = OptionUtilities.getDurationStructure(optionInfoList, today);
                double duration0 = dateStructure[0]==0?dateStructure[1]:dateStructure[0];
                duration = duration0;
                var call = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration),"认购").OrderBy(x=>Math.Abs(x.strike-etfPrice)).Where(x=>x.startDate<=today).ToList();
                var callATM = call[0];
                var callPrice=Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(callATM.optionCode, today,today);
                double callImpv= ImpliedVolatilityUtilities.ComputeImpliedVolatility(callATM.strike, duration / 252.0, 0.04, 0, callATM.optionType, callPrice[0].close, etfPrice);
                var put = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽").OrderBy(x => Math.Abs(x.strike - callATM.strike)).ToList();
                var putATM = put[0];
                var putPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(putATM.optionCode, today, today);
                double putImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(putATM.strike, duration / 252.0, 0.04, 0, putATM.optionType, putPrice[0].close, etfPrice);
                if (callImpv*putImpv==0)
                {
                    volThisMonth = callImpv + putImpv;
                }
                else
                {
                    volThisMonth = (callImpv + putImpv) / 2;
                }
                //获取当日期限结构,选取下月合约,若下月合约不存在，就获取季月合约
                double duration1 = dateStructure[0] == 0 ? dateStructure[2] : dateStructure[1];
                duration = duration1;
                call = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认购").OrderBy(x => Math.Abs(x.strike - etfPrice)).Where(x => x.startDate <= today).ToList();
                callATM = call[0];
                callPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(callATM.optionCode, today, today);
                callImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(callATM.strike, duration / 252.0, 0.04, 0, callATM.optionType, callPrice[0].close, etfPrice);
                put = OptionUtilities.getOptionListByOptionType(OptionUtilities.getOptionListByDuration(optionInfoList, today, duration), "认沽").OrderBy(x => Math.Abs(x.strike - callATM.strike)).ToList();
                putATM = put[0];
                putPrice = Platforms.container.Resolve<OptionDailyRepository>().fetchFromLocalCsvOrWindAndSave(putATM.optionCode, today, today);
                putImpv = ImpliedVolatilityUtilities.ComputeImpliedVolatility(putATM.strike, duration / 252.0, 0.04, 0, putATM.optionType, putPrice[0].close, etfPrice);
                if (callImpv * putImpv == 0)
                {
                    volNextMonth = callImpv + putImpv;
                }
                else
                {
                    volNextMonth = (callImpv + putImpv) / 2;
                }
                if (duration0 >= step)
                {
                    optionVol[i] = Math.Sqrt(step / duration0) * volThisMonth;
                }
                else if ((duration0 < step && duration1 > step))
                {
                    optionVol[i] = Math.Sqrt((duration1 - step) / (duration1 - duration0)) * volThisMonth + Math.Sqrt((step - duration0) / (duration1 - duration0)) * volNextMonth;
                }
                else if (duration1 <= step)
                {
                    optionVol[i] = volNextMonth;
                }
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
