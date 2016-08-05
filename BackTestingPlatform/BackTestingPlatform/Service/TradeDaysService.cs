using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Service
{
    class TradeDaysService
    {
        TradeDaysRepository tradeDaysRepository = Platforms.container.Resolve<TradeDaysRepository>();

        /// <summary>
        /// 获取tradeDays到内存，先后尝试从本地文件，万德获取
        /// </summary>
        public void loadTradeDays()
        {
            var days = tradeDaysRepository.fetchFromLocalFile(
                Constants.TRADE_DAY_START, Constants.TRADE_DAY_END);
            if (days == null)
            {
                days = tradeDaysRepository.fetchFromWind(
                 Constants.TRADE_DAY_START, Constants.TRADE_DAY_END);

                if (days == null)
                {
                    Console.WriteLine("[ERROR] fetch TradeDays failure!");
                }

                tradeDaysRepository.saveToLocalFile(days); //写入csv
                Console.WriteLine("[INFO] TradeDays saved to local file.");
            }

            Platforms.BasicInfo["TradeDays"] = days;
        }
    }
}
