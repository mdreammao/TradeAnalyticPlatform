using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using BackTestingPlatform.Model;
using BackTestingPlatform.DataAccess.Option;
using BackTestingPlatform.Utilities;
using BackTestingPlatform.Model.Option;
using BackTestingPlatform.Model.Common;
using System.IO;
using System.Data;

namespace BackTestingPlatform.Service.Option
{
   


    public class OptionInfoService
    {

        OptionInfoRepository optionInfoRepository = Platforms.container.Resolve<OptionInfoRepository>();

        /// <summary>
        /// 加载OptionInfo到内存,先后尝试从本地csv，wind获取
        /// </summary>
        /// <param name="underlyingCode"></param>
        /// <param name="market"></param>
        public void loadOptionInfo(string underlyingCode, string market)
        {
            List<OptionInfo> optionInfos;
            int daysUpdateRound = 1;    //CacheData更新周期间隔
            var filePath = FileUtils.GetCacheDataFilePathThatLatest(OptionInfoRepository.PATH_KEY);
            var daysdiff = FileUtils.GetCacheDataFileDaysPastTillToday(filePath);
            if (daysdiff > daysUpdateRound)
            {   //CacheData太旧，需要远程更新，然后保存到本地CacheData目录
                optionInfos = optionInfoRepository.fetchFromWind(underlyingCode, market);
                optionInfoRepository.saveToLocalFile(optionInfos);



            }
            else
            {   //CacheData不是太旧，直接读取
                optionInfos = optionInfoRepository.fetchAllFromLocalFile(filePath, underlyingCode, market);
            }

            //加载到内存缓存
            Caches.put("OptionInfos", optionInfos);
            Console.WriteLine(optionInfos);


        }

    }
}
