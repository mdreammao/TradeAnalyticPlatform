using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Monitor.Model;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Utilities.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.Monitor.Option
{
    public class VolSurface
    {
        DateTime today = new DateTime();
        List<OptionInfo> optionList = new List<OptionInfo>();
        Dictionary<string, Level1Data> dataList = new Dictionary<string, Level1Data>();
        List<string> dataListKey = new List<string>();
        Dictionary<string, OptionInfo> optionInfo = new Dictionary<string, OptionInfo>();
        double duartion;
        //Dictionary<string, double> optionDuration = new Dictionary<string, double>();
        double rate;
        string ETF = "510050.SH";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="todayInt"></param>
        /// <param name="riskFreeRate"></param>
        public VolSurface(int todayInt=0,double riskFreeRate=0.043)
        {
            if (todayInt==0)
            {
                today = DateTime.Now;
            }
            else
            {
                today = Kit.ToDate(todayInt);
            }
            optionList =getExistingOption(today);
            //获取今日期权的到期日期
            var dateStructure = OptionUtilities.getDurationStructure(optionList, today);
            //存储今日期权的基本信息
            optionInfo = optionList.ToDictionary(x => x.optionCode, y => y);
            rate = riskFreeRate;
            duartion = dateStructure[0];
            getOptionData(dateStructure[0],today);
            while (DateTime.Now.TimeOfDay<=new TimeSpan(11,30,0))
            {
                GetVolAsync();
            }
        }

        public void computeVol()
        {
            var etfPrice = dataList[ETF].last;
            foreach (var code  in dataListKey)
            {
                var option = dataList[code];
                var optionCode = code;
                if (optionCode!=ETF) 
                {
                    var info = optionInfo[optionCode];
                    var ask = option.ask1;
                    var bid = option.bid1;
                    var mid = (ask + bid) / 2;
                    var strike = info.strike;
                    var type = info.optionType;
                    var vol = ImpliedVolatilityUtilities.ComputeImpliedVolatility(strike: strike, duration: duartion, riskFreeRate: rate, StockRate: 0, optionType: type, optionPrice: mid, underlyingPrice: etfPrice);
                    Console.WriteLine("期权：{0},波动率：{1}", info.optionName, vol);
                }
            }
        }

        public void getOptionData(double duration,DateTime today)
        {
            var optionListNow= OptionUtilities.getOptionListByDuration(getExistingOption(today),today, duration);
            List<string> code = new List<string>();
            dataListKey = code;
            foreach (var item in optionListNow)
            {
                code.Add(item.optionCode);
            }
            code.Add(ETF);
            Execute(code);
           
        }

        
        async Task<string> GetVolAsync()
        {
            computeVol();
            await Task.Delay(TimeSpan.FromSeconds(10));
            return "delay!";
        }

        /// <summary>
        /// 万德的查询函数
        /// </summary>
        /// <param name="code">查询对象代码</param>
        private void Execute(List<string> code)
        {
            WindAPI w = new WindAPI();
            w.start();
            int errorId = 0;
            string codeList = "";
            foreach (var item in code)
            {
                codeList += item + ",";
                dataList.Add(item, new Level1Data());
            }
            codeList.Remove(codeList.Length - 1, 1);

            ulong reqId = w.wsq(ref errorId,codeList, "rt_ask1,rt_asize1,rt_bid1,rt_bsize1,rt_latest,rt_time", "", myCallback);
        }


        public void myCallback(ulong reqId, WindData wd)
        {
            //用户代码区域
            //订阅返回数据存放在WindData参数wd中，可以对其进行分析操作
            var data = wd.data as double[];
            var codeList = wd.codeList as string[];
            if (data!=null)
            {
                int length = data.Length / 6;
                double last, ask1, askv1, bid1, bidv1;
                TimeSpan now;
                for (int i = 0; i < length; i++)
                {
                    ask1 = data[i * 6];
                    askv1 = data[i * 6 + 1];
                    bid1 = data[i * 6 + 2];
                    bidv1 = data[i * 6 + 3];
                    last = data[i * 6 + 4];
                    int time = (int)data[i * 6 + 5] ;
                    var hour = time / 10000;
                    var minute = time % 10000 / 100; 
                    var second = time % 100;
                    now = new TimeSpan(hour, minute, second);
                    dataList[codeList[i]] = new Level1Data() { last = last, ask1 = ask1, askv1 = askv1, bid1 = bid1, bidv1 = bidv1,time=now};
                    // Console.WriteLine("time: {6}, localtime: {7}, code: {5}, last: {0},ask1: {1},askv1: {2},bid1: {3},bidv1: {4}", last, ask1, askv1, bid1, bidv1,codeList[i],now.ToString(),DateTime.Now.TimeOfDay.ToString());
                }
            }
        }
        /// <summary>
        /// 获取当日存续的未经过调整的合约
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
                if (item.startDate <= today && item.endDate >= today && item.contractMultiplier == 10000)
                {
                    optionList.Add(item);
                }
            }
            return optionList;
        }



    }
}
