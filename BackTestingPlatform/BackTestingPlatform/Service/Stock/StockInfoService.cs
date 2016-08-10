using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingPlatform.Model;

namespace BackTestingPlatform.Service.Stock
{
    /// <summary>
    /// 检查本地股票信息数据库，如权息调整因子，股票基本信息等
    /// 如需要更新，则从Wind拉取并更新储存在本地，若无需更新，直接从本地读取
    /// </summary>
    class StockInfoService
    {
        /*
        OptionMinuteDataRepository optionMinuteDataRepository = Platforms.container.Resolve<OptionMinuteDataRepository>();

        public List<OptionMinuteData> loadOptionMinuteData(string optionCode, DateTime date)
        {
            List<OptionMinuteData> optionData;
            var filePath = FileUtils.GetCacheDataFileByCodeAndDate(OptionMinuteDataRepository.PATH_KEY, optionCode, date);
            //若本地文件存在，则从本地读取否者先从万德或者数据库中读取
            if (File.Exists(filePath))
            {
                optionData = optionMinuteDataRepository.fetchAllFromLocalFile(filePath);
            }
            else
            {
                optionData = optionMinuteDataRepository.fetchMinuteDataFromWind(optionCode, date);
                if (optionData != null)
                {
                    optionMinuteDataRepository.saveToLocalFile(optionData, filePath);
                }
            }
            return optionData == null ? null : optionData;
        }
    
         * */
    }
}
