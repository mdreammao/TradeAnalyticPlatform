using BackTestingPlatform.Core;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.DataAccess.Common
{
    public abstract class SequentialDataRepository<T> where T : new()
    {
        const string PATH_KEY = "CacheData.Path.Basic";

        public abstract List<T> fetchFromLocalCsv();
        public abstract List<T> fetchFromWind();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appendMode">是否为append模式，否则为new模式</param>
        /// <param name="localCsvExpration">CacheData更新周期间隔</param>
        /// <param name="tag"></param>
        public void fetchFromLocalCsvOrWindAndUpdateAndCache(int localCsvExpration, bool appendMode = false, String tag = null)
        {

            if (tag == null) tag = typeof(T).Name;
            List<T> data=null;
            var filePath = FileUtils.GetCacheDataFilePathThatLatest(PATH_KEY);
            var daysdiff = FileUtils.GetCacheDataFileDaysPastTillToday(filePath);
            if (daysdiff > localCsvExpration)
            {   //CacheData太旧，需要远程更新，然后保存到本地CacheData目录
                Console.WriteLine("本地csv文件已过期，尝试Wind读取新数据...");
                try
                {
                    data = fetchFromWind();
                }
                catch(Exception e)
                {
                    Console.WriteLine("从Wind读取数据失败！");
                    Console.WriteLine(e);
                }
                try
                {
                    //data = saveToLocalCsvFile();
                }
                catch (Exception e)
                {
                    Console.WriteLine("从Wind读取数据失败！");
                    Console.WriteLine(e);
                }

            }
            else
            {   //CacheData不是太旧，直接读取
                Console.WriteLine("从本地cs文件读取数据...");
                try
                {
                    data = fetchFromLocalCsv();
                }
                catch (Exception e)
                {
                    Console.WriteLine("从本地cs文件读取数据失败！");
                    Console.WriteLine(e);
                }
               
            }

            //加载到内存缓存
            Caches.put(tag, data);
            Console.WriteLine("已将{0}加载到内存缓存.",tag);

        }

      
        /// <summary>
        /// 将数据以csv文件的形式保存到CacheData文件夹下的预定路径
        /// </summary>
        public void saveToLocalCsvFile(IList<T> data, string path,bool appendMode=false, string tag = null)
        {
            if (tag == null) tag = typeof(T).Name;
            if (data == null || data.Count == 0)
            {
                Console.WriteLine("Nothing to save!");
                return;
            }
            var dt = DataTableUtils.ToDataTable(data);
            //var path = FileUtils.GetCacheDataFilePath(PATH_KEY, tag, code, date.ToString("yyyyMMdd"));
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
