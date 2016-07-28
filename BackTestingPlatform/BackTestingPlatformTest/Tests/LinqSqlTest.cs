using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;

namespace BackTestingPlatform.Tests
{
    class LinqSqlTest
    {
        public static void testSqlDataReader()
        {
           
            var sql = @"
select * 
from[TradeMarket201604].[dbo].[MarketData_A1605_DCE]
where [tdate]>=20160422 and [tdate]<=20160422";
            string connStr = ConfigurationManager.ConnectionStrings["corp1"].ConnectionString;
            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            SqlCommand cmd = new SqlCommand(sql,conn);
            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
              
            }

        }

        public static void testLinq()
        {
            //DataClasses1DataContext dataContext = new DataClasses1DataContext();
           //TODO: ...
        }

        public static void testDataSet()
        {
            var sql = @"
select * 
from[TradeMarket201605].[dbo].[MarketData_A1605_DCE]
where [tdate]>=20160122 and [tdate]<=20160522";
            string connStr = ConfigurationManager.ConnectionStrings["local"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                SqlDataAdapter myDataAdapter = new SqlDataAdapter(sql, conn);
                DataSet ds = new DataSet();
                myDataAdapter.Fill(ds);

                DataTable dt = ds.Tables[0];
                foreach (DataRow row in dt.Rows)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        Console.Write("{0,10}",row[col]); //遍历表中的每个单元格

                    }
                    Console.WriteLine();
                }

            }
        }
    }
}
