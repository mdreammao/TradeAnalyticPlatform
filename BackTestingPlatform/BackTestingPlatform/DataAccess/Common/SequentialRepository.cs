using BackTestingPlatform.Model.Common;
using BackTestingPlatform.Utilities;
using NLog;
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
    /// 时间序列数据的Repository
    /// 
    /// </summary>
    /// <typeparam name="T">时间序列实体类</typeparam>
    public abstract class SequentialRepository<T> where T : Sequential, new()
    {
        Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 由DataTable中的行向实体类的转换函数的默认实现。
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public virtual T toEntityFromCsv(DataRow row)
        {
            return DataTableUtils.CreateItemFromRow<T>(row);
        }

        /// <summary>
        /// 将数据以csv文件的形式保存到CacheData文件夹下的预定路径
        /// </summary>
        /// <param name="data">要保存的数据</param>
        /// <param name="path">读写文件路径</param>
        /// <param name="appendMode">是否为追加的文件尾部模式，否则是覆盖模式</param>
        public virtual void saveToLocalCsv(string path,IList<T> data, bool appendMode = false)
        {            
            if (data == null || data.Count == 0)
            {
                log.Warn("没有任何内容可以保存到csv！");
                return;
            }
            var dt = DataTableUtils.ToDataTable(data);
            try
            {
                var s = (File.Exists(path)) ? "覆盖" : "新增";
                CsvFileUtils.WriteToCsvFile(path, dt, appendMode);
                log.Debug("文件已{0}：{1} ", s, path);
            }
            catch (Exception e)
            {
                log.Error(e, "保存到本地csv文件失败！({0})",path);
            }
            
        }

        /// <summary>
        ///  尝试从本地csv文件获取数据,可能会抛出异常
        /// </summary>
        /// <param name="path">读写文件路径</param>
        /// <returns></returns>
        public virtual List<T> readFromLocalCsv(string path)
        {
            if (!File.Exists(path))
            {
                log.Debug("文件路径{0}不存在，无法读取！", path);
                return null;
            }
            DataTable dt = CsvFileUtils.ReadFromCsvFile(path);
            if (dt == null) return null;
            return dt.AsEnumerable().Select(toEntityFromCsv).ToList();
        }
    }
}
