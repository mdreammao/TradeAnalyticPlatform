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
    /// <summary>
    /// 按每年存取时间序列数据的Repository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SequentialByYearRepository<T> where T : Sequential, new()
    {
        const string PATH_KEY = "CacheData.Path.SequentialByYear";

        /// <summary>
        /// 由CSV读出的DataTable中的行向实体类的转换函数的默认实现。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual T toEntityFromCsv(DataRow row)
        {
            return DataTableUtils.CreateItemFromRow<T>(row);
        }

        /// <summary>
        /// 尝试从Wind获取数据,可能会抛出异常
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected abstract List<T> readFromWind(string code, DateTime date1, DateTime date2, string tag = null, IDictionary<string, object> options = null);


        /// <summary>
        /// 尝试从默认MSSQL源获取数据,可能会抛出异常
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        protected abstract List<T> readFromDefaultMssql(string code, DateTime date1, DateTime date2, string tag = null, IDictionary<string, object> options = null);



        private string _buildCacheDataFilePath(string code, DateTime date1, DateTime date2, string tag)
        {
            return FileUtils.GetCacheDataFilePath(PATH_KEY, new Dictionary<string, string>
            {
                ["tag"] = tag,
                ["code"] = code,
                ["date1"] = date1.ToString("yyyyMMdd"),
                ["date2"] = date1.ToString("yyyyMMdd")
            });
        }

        /// <summary>
        /// 尝试从本地csv文件获取数据,可能会抛出异常
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <param name="tag"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public List<T> readFromLocalCsv(string code, DateTime date1, DateTime date2, string tag = null, IDictionary<string, object> options = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            var filePath = _buildCacheDataFilePath(code, date1, date2, tag);

            DataTable dt = CsvFileUtils.ReadFromCsvFile(filePath);
            if (dt == null) return null;
            return dt.AsEnumerable().Select(toEntityFromCsv).ToList();
        }

        /// <summary>
        /// 尝试从本地csv文件，Wind获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsv(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, true, false, false, false);
        }

        /// <summary>
        /// 尝试从Wind获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromWind(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, false, true, false, false);
        }

        /// <summary>
        /// 尝试从默认MSSQL源获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromMssql(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, false, false, true, false);
        }

        /// <summary>
        /// 先后尝试从本地csv文件，Wind获取数据。若无本地csv，则保存到CacheData文件夹。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrWindAndSave(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, true, true, false, true);
        }
        /// <summary>
        /// 先后尝试从本地csv文件，默认MSSQL数据库源获取数据。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrMssql(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, true, false, true, false);
        }

        /// <summary>
        /// 先后尝试从本地csv文件，默认MSSQL数据库源获取数据。若无本地csv，则保存到CacheData文件夹。
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromLocalCsvOrMssqlAndSave(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, true, false, true, true);
        }

        /// <summary>
        /// 尝试Wind获取数据。然后将数据覆盖保存到CacheData文件夹
        /// </summary>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<T> fetchFromWindAndSave(string code, int year, string tag = null, IDictionary<string, object> options = null)
        {
            return fetch0(code, year, tag, options, false, true, false, true);
        }

        private List<T> fetch0(string code, int year, string tag, IDictionary<string, object> options, bool tryCsv, bool tryWind, bool tryMssql0, bool saveToCsv)
        {
            if (tag == null) tag = typeof(T).ToString();
            List<T> result = null;
            bool csvHasData = false;
            var date1 = new DateTime(year, 1, 1);
            var date2 = new DateTime(year, 12, 31);
            if (tryCsv)
            {
                //尝试从csv获取
                Console.WriteLine("尝试从csv获取...");
                try
                {
                    result = readFromLocalCsv(code, date1, date2, tag, options);
                }
                catch (Exception e)
                {
                    Console.WriteLine("尝试从csv获取失败！");
                    Console.WriteLine(e);
                }
                if (result != null) csvHasData = true;
            }
            if (result == null && tryWind)
            {
                //尝试从Wind获取
                Console.WriteLine("尝试从Wind获取...");
                try
                {
                    result = readFromWind(code, date1, date2, tag, options);
                }
                catch (Exception e)
                {
                    Console.WriteLine("尝试从Wind获取失败！");
                    Console.WriteLine(e);
                }
            }
            if (result == null && tryMssql0)
            {
                try
                {
                    //尝试从默认MSSQL源获取
                    Console.WriteLine("尝试从默认MSSQL源获取...");
                    result = readFromDefaultMssql(code, date1, date2, tag, options);
                }
                catch (Exception e)
                {
                    Console.WriteLine("尝试从默认MSSQL源获取失败！");
                    Console.WriteLine(e);
                }

            }
            if (!csvHasData && result != null && saveToCsv)
            {
                //如果数据不是从csv获取的，可保存至本地，存为csv文件
                Console.WriteLine("正在保存到本地csv文件...");
                saveToLocalCsvFile(result, code, date1, date2, tag);
            }
            Console.WriteLine("获取{0}数据列表成功.共{1}行.", tag, result.Count);
            return result;
        }


        /// <summary>
        /// 将数据以csv文件的形式保存到CacheData文件夹下的预定路径
        /// </summary>
        /// <param name="data"></param>
        /// <param name="code"></param>
        /// <param name="date"></param>
        /// <param name="tag"></param>
        public void saveToLocalCsvFile(IList<T> data, string code, DateTime date1, DateTime date2, string tag = null)
        {
            if (tag == null) tag = typeof(T).ToString();
            if (data == null || data.Count == 0)
            {
                Console.WriteLine("Nothing to save!");
                return;
            }
            var dt = DataTableUtils.ToDataTable(data);
            var path = _buildCacheDataFilePath(code, date1, date2, tag);

            try
            {
                var s = (File.Exists(path)) ? "覆盖" : "新增";
                CsvFileUtils.WriteToCsvFile(path, dt);
                Console.WriteLine("文件已{0}：{1} ", s, path);
            }
            catch (Exception e)
            {
                Console.WriteLine("保存到本地csv文件失败！");
                Console.WriteLine(e);
            }


        }
    }
}
