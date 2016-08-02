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
        /// 如果目标csv文件已存在，会覆盖
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="filePath"></param>
        public static void WriteToCsvFile(DataTable dt, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field =>
  string.Concat("\"", field.ToString().Replace("\"", "\"\""), "\""));
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// http://stackoverflow.com/a/27705485
        /// 简单的csv读取处理，不支持含逗号的内容。
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="firstRowAsHeader">csv文件第一行是否作为header</param>
        /// <returns></returns>
        public static DataTable ReadfromCsvFile(string filePath,bool firstRowAsHeader=true)
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
    }


   
}
