using BackTestingPlatform.DataAccess;
using BackTestingPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            MyPlatform.init();
            WsiDao dao = new WsiDao();

           var ddd= 
            dao.fetch("510050.SH", new DateTime(2015, 7, 26, 9, 0, 0), new DateTime(2016, 7, 26, 15, 0, 0));
            System.Console.ReadKey();
        }
    }
}
