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
    /// <summary>
    /// 检查本地股票信息数据库，如权息调整因子，股票基本信息等
    /// 如需要更新，则从Wind拉取并更新储存在本地，若无需更新，直接从本地读取
    /// </summary>
    class StockInfoService
    {
        ASharesInfoRepository ASharesInfoRepository = Platforms.container.Resolve<ASharesInfoRepository>();

        public List<ASharesInfo> loadASharesInfoData(string stockCode, DateTime date)
        {
  //          List<ASharesInfo> ASharesInfoData;
  //          var filePath = FileUtils.GetCacheDataFileByCodeAndDate(ASharesInfoRepository.PATH_KEY, stockCode, date);
  //          //若本地文件存在，则从本地读取否者先从万德或者数据库中读取
  //          if (File.Exists(filePath))
  //          {
  //       //       ASharesInfoData = ASharesInfoRepository.fetchAllFromLocalFile(filePath);
  //          }
  //          else
  //          {
  //         //     ASharesInfoData = ASharesInfoRepository.fetchASharesInfoDataFromWind(stockCode, date);
  //        //      if (ASharesInfoData != null)
  //              {
  //     //             ASharesInfoRepository.saveToLocalFile(ASharesInfoData, filePath);
  //              }
  //          }
  ////          return ASharesInfoData == null ? null : ASharesInfoData;
            return null;
        }
    
        
    }
}
