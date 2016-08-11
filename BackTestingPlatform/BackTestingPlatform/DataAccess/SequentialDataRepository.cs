using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess
{
    public abstract class SequentialDataRepository<T> where T : Sequential
    {
        //int CacheDataUpdateCycleDayLength = 1;    //CacheData有效期天数
        //bool CacheDataShouldPutToMemCache = true;   //
        const string PATH_KEY = "CacheData.Path.Sequential";

        /// <summary>
        /// 由DataTable中的行向实体类的转换
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public abstract T toEntity(DataRow row);

        /// <summary>
        ///  尝试从Wind获取数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public abstract List<T> fetchFromWind(string code, string date);

        /// <summary>
        ///  尝试从本地csv文件获取数据
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsv(string code, string date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            var filePath = FileUtils.GetCacheDataFilePath(PATH_KEY, tag, code, date);
            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            if (dt == null) return null;
            return dt.AsEnumerable().Select(toEntity).ToList();
        }

        /// <summary>
        /// 先后尝试从本地csv文件，Wind获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrWind(string code, string date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            //尝试从csv获取
            List<T> result = fetchFromLocalCsv(code, date, tag);

            if (result == null)
            {
                //尝试从Wind获取
                result = fetchFromWind(code, date);
            }
            return result;
        }

        /// <summary>
        /// 先后尝试从本地csv文件，Wind获取数据。若无本地csv，则保存到CacheData文件夹。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrWindAndSave(string code, string date, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            //尝试从csv获取
            List<T> result = fetchFromLocalCsv(code, date, tag);

            if (result == null)
            {
                //尝试从Wind获取
                result = fetchFromWind(code, date);
                //并保存
                saveToLocalCsvFile(result, code, date, tag);
            }
            return result;
        }

        /// <summary>
        /// 将数据以csv文件的形式保存到CacheData文件夹下的预定路径
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
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
