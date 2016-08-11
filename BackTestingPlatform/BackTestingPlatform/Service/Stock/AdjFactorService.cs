using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model.Stock;
using BackTestingPlatform.Utilities;
using Autofac;
using System.IO;

namespace BackTestingPlatform.Service.Stock
{
    class AdjFactorService
    {
        AdjFactorRepository adjFactorRepository = Platforms.container.Resolve<AdjFactorRepository>();

        /// <summary>
        /// 加载AdjFactor到内存,先后尝试从本地csv，wind获取
        /// </summary>
        /// <param name="stockCode"></param>
        /// <param name="market"></param>
        public void loadAdjFactor(string stockCode ,DateTime startTime,DateTime endTime)
        {
            List<AdjFactor> adjFactor;
            int daysUpdateRound = 1;    //CacheData更新周期间隔
            var filePath = FileUtils.GetCacheDataFileThatLatest(AdjFactorRepository.PATH_KEY);
            var daysdiff = FileUtils.GetCacheFileDaysPastTillToday(filePath);
            if (daysdiff > daysUpdateRound)
            {   //CacheData太旧，需要远程更新，然后保存到本地CacheData目录
                adjFactor = adjFactorRepository.fetchFromWind(stockCode, startTime,endTime);
                adjFactorRepository.saveToLocalFile(adjFactor, filePath);
            }
            else
            {   //CacheData不是太旧，直接读取
                adjFactor = adjFactorRepository.fetchAllFromLocalFile(filePath);
            }

            Platforms.BasicInfo["AdjFactor"] = adjFactor;
            Console.WriteLine(adjFactor);
        }
    }
}
