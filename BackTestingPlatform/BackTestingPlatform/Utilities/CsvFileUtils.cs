using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform.Utilities
{
    public static class CsvFileUtils
    {
        /// <summary>
        ///  DataTable -> CSV 如果目标csv文件已存在，会覆盖
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="filePath"></param>
        public static void WriteToCsvFile(string filePath,DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray
                    .Select(toReadableString).Select(toDoubleQuotedString);
                sb.AppendLine(string.Join(",", fields));
            }
            var dirPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            //若文件路径不存在则生成该文件夹
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        static string toReadableString(object cell)
        {
            if (cell is DateTime)
            {
                return ((DateTime)cell).ToString("yyyyMMddhhmmss");
            }
            return cell.ToString();
        }

        static string toDoubleQuotedString(string src)
        {
            return string.Concat("\"", src.Replace("\"", "\"\""), "\"");          
        }



        /// <summary>
        /// http://stackoverflow.com/a/27705485
        /// CSV -> DataTable 简单的csv读取处理，不支持含逗号的内容。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="firstRowAsHeader">csv文件第一行是否作为header</param>
        /// <returns></returns>
        public static DataTable ReadFromCsvFile(string filePath,bool firstRowAsHeader=true)
        {
            DataTable dt = new DataTable();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    if (!sr.EndOfStream && firstRowAsHeader)
                    {
                        string[] headers = sr.ReadLine().Split(',');
                        foreach (string header in headers)
                        {
                            dt.Columns.Add(header);
                        }
                    }
                   
                    while (!sr.EndOfStream)
                    {
                        string[] rows = sr.ReadLine().Split(',');
                        dt.Rows.Add(rows);
                    }
                }
            }
            return dt;
        }
        /// <summary>
        /// 将values转换为csv文件的一行，包含一些默认的类型转换，例如：
        /// toCsvFileLine("a",2.1,DateTime1)=="\"a\",\"2.1\",\"20160804093200\"";
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string toCsvFileLine(params object[] values)
        {
            if (values == null) return "";
            var res = new StringBuilder();
            foreach (var val in values)
            {
                var s = "";
                if (val is DateTime)                
                    s = ((DateTime)val).ToString("yyyyMMddhhmmss");
                s = val.ToString();
                res.Append(s);
            }
            return res.ToString();
        }
    }


   
}
