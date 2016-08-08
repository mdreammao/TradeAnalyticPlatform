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
        public void loadOptionInfo(string underlyingCode="510050.SH", string market="sse")
        {
            List<OptionInfo> optionInfos;
            int daysUpdateRound = 1;    //CacheData更新周期间隔
            var filePath = FileUtils.GetCacheDataFileThatLatest(OptionInfoRepository.PATH_KEY);
            var daysdiff = FileUtils.GetCacheFileDaysPastTillToday(filePath);
            if (daysdiff > daysUpdateRound)
            {   //CacheData太旧，需要远程更新，然后保存到本地CacheData目录
                optionInfos = optionInfoRepository.fetchFromWind(underlyingCode, market);
                optionInfoRepository.saveToLocalFile(optionInfos);
               
            }
            else
            {   //CacheData不是太旧，直接读取
                optionInfos = optionInfoRepository.fetchAllFromLocalFile(filePath, underlyingCode, market);
            }

            Platforms.BasicInfo["OptionInfos"] = optionInfos;
            Console.WriteLine(optionInfos);
            

        }

    }
}
