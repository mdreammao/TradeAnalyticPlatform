using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Service
{
    public abstract class SequentialDataRepository<T> where T : Sequential
    {
        //int CacheDataUpdateCycleDayLength = 1;    //CacheData有效期天数
        //bool CacheDataShouldPutToMemCache = true;   //
        const string PATH_KEY = "CacheData.Path.Sequential";

        public abstract T toEntity(DataRow row);
        public abstract List<T> fetchFromWind(string code, string date);

        public List<T> fetchFromLocalCsv(string code, string date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            var filePath = FileUtils.GetCacheDataFilePath(PATH_KEY, tag, code, date);
            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            if (dt == null) return null;
            return dt.AsEnumerable().Select(toEntity).ToList();
        }

        public List<T> fetchFromLocalCsvOrWind(string code, string date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            //尝试从csv获取
            List<T> result = fetchFromLocalCsv(code,date,tag);
           
            if (result == null)
            {         
                //尝试从Wind获取
                result = fetchFromWind(code, date);
            }
            return result;
        }

        public void saveToLocalCsvFile(IList<T> data, string code, string date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            var dt = DataTableUtils.ToDataTable(data);
            var path = FileUtils.GetCacheDataFilePath(PATH_KEY, tag, code, date);
            CsvFileUtils.WriteToCsvFile(path, dt);
            var s = (File.Exists(path)) ? "old file has been overwritten." : "";
            Console.WriteLine("{0} saved! {1}", path);
        }
    }
}
