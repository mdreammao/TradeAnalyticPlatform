using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
namespace BackTestingPlatformTest.Tests
{
    class LinqSqlTest
    {
        public static void testSqlDataReader()
        {
           
            var sql = @"
select * 
from[TradeMarket201604].[dbo].[MarketData_510050_SH]
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
            DataClasses1DataContext dataContext = new DataClasses1DataContext();
           //TODO: ...
        }

        public static void testDataSet()
        {
            string connStr = ConfigurationManager.ConnectionStrings["corp1"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                SqlDataAdapter myDataAdapter = new SqlDataAdapter(;

            }
        }
    }
}
