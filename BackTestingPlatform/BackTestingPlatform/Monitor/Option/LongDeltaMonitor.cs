using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Monitor.Option.Model;
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
        List<LongDeltaPair> pairs = new List<LongDeltaPair>();
        DateTime today = new DateTime();
        WindAPI w = Platforms.GetWindAPI();
        double riskFreeRate = 0.02;
        double etfPrice = 0;
        public LongDeltaMonitor(int todayInt)
        {
            today = Kit.ToDate(todayInt);
            optionList = getExistingOption(today);
            optionPrice=getOptionPrice(optionList,ref etfPrice);
            duration = getDuartion(optionPrice);
            volSurface = getVolSurface(optionPrice);
            pairs=getLongDeltaPair(10);
            displayPairs();
        }

        private void displayPairs()
        {
            foreach (var item in pairs)
            {
                var option1 = OptionUtilities.getOptionByCode(optionList, item.option1);
                var option2= OptionUtilities.getOptionByCode(optionList, item.option2);
                var price1 = optionPrice[option1.optionCode];
                var price2 = optionPrice[option2.optionCode];
                double delta = price1.delta - price2.delta;
                double gamma = price1.gamma - price2.gamma;
                double vega = price1.vega - price2.vega;
                double theta = price1.theta - price2.theta;
                Console.WriteLine("代码:{0}, 名称:{1}, 价格:{2} ask:{3}  bid:{4}, 波动率:{5}", option1.optionCode, option1.optionName, optionPrice[option1.optionCode].lastPrice, optionPrice[option1.optionCode].ask, optionPrice[option1.optionCode].bid,price1.impv);
                Console.WriteLine("代码:{0}, 名称:{1}, 价格:{2} ask:{3}  bid:{4}, 波动率:{5}", option2.optionCode, option2.optionName, optionPrice[option2.optionCode].lastPrice, optionPrice[option2.optionCode].ask, optionPrice[option2.optionCode].bid,price2.impv);
                Console.WriteLine("mark:{0}, delta:{1}, gamma:{2}, vega:{3}, theta:{4}, profit:{5}, loss:{6}", item.mark, delta, gamma, vega, theta, item.profit, item.loss);
                Console.WriteLine("==============================================");
            }
        }

        /// <summary>
        /// 两两配对的longdelta期权的筛选
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        private List<LongDeltaPair> getLongDeltaPair(int days)
        {
            List<LongDeltaPair> pairs = new List<LongDeltaPair>();
            double etfUpper = etfPrice * Math.Exp(2*volSurface[days, 500] * Math.Sqrt(days / 252.0));
            double etfLower= etfPrice * Math.Exp(-2*volSurface[days, 500] * Math.Sqrt(days / 252.0));
            //两两循环选择最优配对
            foreach (var item1 in optionPrice)
            {
                OptionGreek option1 = item1.Value;
                double price1 = computeFuturePrice(days, option1.code, etfPrice);
                foreach (var item2 in optionPrice)
                {
                    OptionGreek option2 = item2.Value;
                    if (option1.code!=option2.code && option1.duration>=days && option2.duration>=days)
                    {
                        //选取delta为正的组合开始计算。
                        LongDeltaPair pair = new LongDeltaPair();
                        double profit = 0;
                        double loss = 0;
                        if (option1.delta>option2.delta)
                        {
                            pair.option1 = option1.code;
                            pair.option2 = option2.code;
                            pair.mark=Math.Abs(computeMarkOfPairs(option1.code, option2.code, days, etfUpper, etfLower, ref profit, ref loss));
                            pair.profit = profit;
                            pair.loss = loss;

                        }
                        else
                        {
                            pair.option1 = option2.code;
                            pair.option2 = option1.code;
                            pair.mark =Math.Abs(computeMarkOfPairs(option2.code, option1.code, days, etfUpper, etfLower, ref profit, ref loss));
                            pair.profit = profit;
                            pair.loss = loss;
                        }
                        if (pair.profit>0 && pair.mark>0.5 && pair.profit / Convert.ToDouble(days) * 252.0 > 1)
                        {
                            pairs.Add(pair);
                        }
                    }
                }
            }
            return pairs.OrderBy(f=>-f.mark).ToList();
        }

        /// <summary>
        /// 以收益损失比给期权组合打分
        /// </summary>
        /// <param name="code1"></param>
        /// <param name="code2"></param>
        /// <param name="days"></param>
        /// <param name="upperPrice"></param>
        /// <param name="lowerPrice"></param>
        /// <param name="up"></param>
        /// <param name="low"></param>
        /// <returns></returns>
        private double computeMarkOfPairs(string code1,string code2,int days,double upperPrice,double lowerPrice,ref double up,ref double low)
        {
            double marks = -100;
            double initialValue = optionPrice[code1].lastPrice - optionPrice[code2].lastPrice;
            double upperValue= computeFuturePrice(days, code1, upperPrice)- computeFuturePrice(days, code2, upperPrice);
            double lowerValue = computeFuturePrice(days, code1, lowerPrice) - computeFuturePrice(days, code2, lowerPrice);
            var option2 = OptionUtilities.getOptionByCode(optionList,code2);
            double margin = OptionMargin.ComputeOpenMargin(etfPrice, optionPrice[code2].lastPrice, option2.strike, option2.optionType, 1, etfPrice)+(upperPrice-lowerPrice)/2;
            double capitalOccupied = margin - initialValue;
            up = (upperValue-initialValue)/capitalOccupied;
            low = (lowerValue-initialValue)/capitalOccupied;
            marks = up / low;
            return marks;
        }
        /// <summary>
        /// 估算未来的期权价格
        /// </summary>
        /// <param name="days"></param>
        /// <param name="code"></param>
        /// <param name="etfPrice"></param>
        /// <returns></returns>
        private double computeFuturePrice(int days,string code,double etfPrice)
        {
            double price = 0;
            var option = OptionUtilities.getOptionByCode(optionList,code);
            int x = Convert.ToInt32(optionPrice[code].duration - days);
            int y = Convert.ToInt32(Math.Round(1000 * Math.Log(option.strike / etfPrice), 0) + 500);
            double vol = volSurface[x, y];
            price = ImpliedVolatilityUtilities.ComputeOptionPrice(option.strike, (optionPrice[code].duration - days)/252.0, riskFreeRate, 0, option.optionType, vol, etfPrice);
            return price;
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
