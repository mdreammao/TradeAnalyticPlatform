using Autofac;
using BackTestingPlatform.Core;
using BackTestingPlatform.DataAccess.Stock;
using BackTestingPlatform.Model;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Service.Stock
{
    public class StockMinuteDataService
    {
        StockMinuteDataRepository stockMinuteDataRepository = Platforms.container.Resolve<StockMinuteDataRepository>();

        public List<StockMinuteData> loadStockMinuteData(string stockCode, DateTime date)
        {
            List<StockMinuteData> stockData;
            var filePath = FileUtils.GetCacheDataFileByCodeAndDate(StockMinuteDataRepository.PATH_KEY, stockCode, date);
            //若本地文件存在，则从本地读取否者先从万德或者数据库中读取
            if (File.Exists(filePath))
            {
                stockData = stockMinuteDataRepository.fetchAllFromLocalFile(filePath);
            }
            else
            {
                stockData = stockMinuteDataRepository.fetchMinuteDataFromWind(stockCode, date);
                if (stockData != null)
                {
                    stockMinuteDataRepository.saveToLocalFile(stockData, filePath);
                }
            }
            return stockData == null ? null : stockData;
        }
    }
}
