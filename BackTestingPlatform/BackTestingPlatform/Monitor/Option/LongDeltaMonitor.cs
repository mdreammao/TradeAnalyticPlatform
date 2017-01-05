using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.Monitor.Option
{
    public class LongDeltaMonitor
    {
        List<OptionInfo> optionList = new List<OptionInfo>();
        Dictionary<string, OptionGreek> optionPrice = new Dictionary<string, OptionGreek>();
        double[,] volSurface = new double[181,1001];
        List<double> duration = new List<double>();
        DateTime today = new DateTime();
        DateTime now = new DateTime();
        WindAPI w = Platforms.GetWindAPI();
        double r = 0.04;
        double etfPrice = 0;
        public LongDeltaMonitor(int todayInt)
        {
            today = Kit.ToDate(todayInt);
            optionList = getExistingOption(today);
            now = DateTime.Now;
            optionPrice=getOptionPrice(optionList,ref etfPrice);
            duration = getDuartion(optionPrice);
            volSurface = getVolSurface(optionPrice);
        }

        /// <summary>
        /// 获取近似的波动率曲面
        /// </summary>
        /// <param name="option"></param>
        /// <param name="etfPrice"></param>
        /// <returns></returns>
        private double[,] getVolSurface(Dictionary<string, OptionGreek> option)
        {
            double[,] vol= new double[181, 1001];
            foreach (var item in option)
            {
                int y = Convert.ToInt32(item.Value.coordinate);
                int x = Convert.ToInt32(item.Value.duration);
                vol[x, y] = item.Value.impv;
            }
            foreach (var item in duration)
            {
                int x= Convert.ToInt32(item);
                List<int> yList = new List<int>();
                for (int y = 0; y < 1001; y++)
                {
                    if (vol[x,y]!=0)
                    {
                        yList.Add(y);
                    }
                }
                for (int i = 0; i < yList.Count(); i++)
                {
                    if (i==0)
                    {
                        int y = yList[i];
                        for (int j = 0; j < y; j++)
                        {
                            vol[x, j] = vol[x, y];
                        }
                    }
                    if (i==yList.Count()-1)
                    {
                        int y = yList[i];
                        for (int j = y+1; j < 1001; j++)
                        {
                            vol[x, j] = vol[x, y];
                        }
                    }
                    if (i<yList.Count()-1)
                    {
                        int y1 = yList[i];
                        int y2 = yList[i + 1];
                        for (int j = y1+1; j < y2; j++)
                        {
                            vol[x, j] = Convert.ToDouble((j - y1)) / (y2 - y1) * vol[x, y2] + Convert.ToDouble((y2 - j)) / (y2 - y1) * vol[x, y1];
                        }
                    }

                }
            }
            for (int y = 0; y < 1001; y++)
            {
                for (int i = 0; i < duration.Count(); i++)
                {
                    if (i==0)
                    {
                        int x = Convert.ToInt32(duration[i]);
                        for (int j = 0; j < x; j++)
                        {
                            vol[j, y] = Convert.ToDouble(j) / x * vol[x, y];
                        }
                    }
                    if (i==duration.Count()-1)
                    {
                        int x= Convert.ToInt32(duration[i]);
                        for (int j = x+1; j < 181; j++)
                        {
                            vol[j, y] = vol[x, y];
                        }
                    }
                    if (i<duration.Count()-1)
                    {
                        int x1= Convert.ToInt32(duration[i]);
                        int x2= Convert.ToInt32(duration[i+1]);
                        for (int j = x1+1; j < x2; j++)
                        {
                            vol[j, y] = Convert.ToDouble((j - x1)) / (x2 - x1) * vol[x2, y] + Convert.ToDouble((x2 - j)) / (x2 - x1) * vol[x1, y];
                        }
                    }
                }
            }
            return vol;
        }
        private List<double> getDuartion(Dictionary<string, OptionGreek> option)
        {
            List<double> duration = new List<double>();
            foreach (var item in option)
            {
                if (duration.Contains(item.Value.duration)==false)
                {
                    duration.Add(item.Value.duration);
                }
            }
            duration.Sort();
            return duration;
        }
        
        /// <summary>
        /// 获取期权和50ETF实时快照数据
        /// </summary>
        /// <param name="optionList"></param>
        /// <param name="etfPrice"></param>
        /// <returns></returns>
        private Dictionary<string,OptionGreek> getOptionPrice(List<OptionInfo> optionList,ref double etfPrice)
        {
            Dictionary<string, OptionGreek> option = new Dictionary<string, OptionGreek>();
            string code = "510050.SH";
            for (int i = 0; i < optionList.Count(); i++)
            {
                OptionGreek greek = new OptionGreek();
                greek.code = optionList[i].optionCode;
                greek.duration = DateUtils.GetSpanOfTradeDays(today, optionList[i].endDate);
                option.Add(greek.code, greek);
                code += ","+greek.code;
            }
            WindData wd=w.wsq(code, "rt_last,rt_ask1,rt_asize1,rt_bid1,rt_bsize1,rt_delta,rt_gamma,rt_vega,rt_theta,rt_imp_volatility", "");
            if (wd.data is double[])
            {
                double[] dataList = (double[])wd.data;
                int length = dataList.Length / 10;
                etfPrice = dataList[0];
                for (int i = 1; i < length; i++)
                {
                    string optionCode = optionList[i-1].optionCode;
                    double strike = optionList[i - 1].strike;
                    OptionGreek greek = option[optionCode];
                    greek.lastPrice = dataList[i * 10 + 0];
                    greek.ask = dataList[i * 10 + 1];
                    greek.askv = dataList[i * 10 + 2];
                    greek.bid = dataList[i * 10 + 3];
                    greek.bidv = dataList[i * 10 + 4];
                    greek.delta = dataList[i * 10 + 5];
                    greek.gamma = dataList[i * 10 + 6];
                    greek.vega = dataList[i * 10 + 7];
                    greek.theta = dataList[i * 10 + 8];
                    greek.impv = dataList[i * 10 + 9];
                    greek.coordinate =Math.Round(1000 * Math.Log(strike / etfPrice),0)+500;
                    option[optionCode] = greek;

                }
                
            }
           
            return option;
        }

        /// <summary>
        /// 获取万德的实时数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private double getLastPrice(string code)
        {
            WindData wd = w.wsq(code, "rt_last", "");
            if (wd.data is double[])
            {
                double[] dataList = (double[])wd.data;
                return dataList.Last();
            }
            return 0;

        }


        /// <summary>
        /// 获取当日存续的合约
        /// </summary>
        /// <param name="today"></param>
        /// <returns></returns>
        private List<OptionInfo> getExistingOption(DateTime today)
        {
            List<OptionInfo> optionList = new List<OptionInfo>();
            var repo = Platforms.container.Resolve<OptionInfoRepository>();
            var optionInfoList = repo.fetchFromLocalCsvOrWindAndSaveAndCache(0);
            Caches.put("OptionInfo", optionInfoList);
            foreach (var item in optionInfoList)
            {
                if (item.startDate<=today && item.endDate>=today && item.contractMultiplier==10000)
                {
                    optionList.Add(item);
                }
            }
            return optionList;
        }
    }
}
