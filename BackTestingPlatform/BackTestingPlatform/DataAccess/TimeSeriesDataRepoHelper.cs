using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    public class TimeSeriesDataRepoHelper
    {        
        public static void saveToLocalCsvFile<T>(IList<T> data, string appKey,string type, string code, DateTime date) where T: Sequential
        {
            var dt = DataTableUtils.ToDataTable(data);
            var path = FileUtils.GetCacheDataFilePath(appKey, type, code, date.ToString("yyyyMMdd"));
            CsvFileUtils.WriteToCsvFile(path, dt);
            Console.WriteLine("{0} saved!", path);
        }

    }
}
