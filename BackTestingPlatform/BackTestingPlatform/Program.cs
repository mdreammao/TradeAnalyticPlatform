using Autofac;
using BackTestingPlatform.DataAccess;
using BackTestingPlatform.Core;
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

            Platforms.Initialize();
         
            var repo = Platforms.container.Resolve<WsiDataRepository>();
            var d=repo.fetch("510050.SH", new DateTime(2015, 7, 26, 9, 0, 0), new DateTime(2016, 7, 26, 15, 0, 0));
            Console.WriteLine("");
            System.Console.ReadKey();
        }
    }
}
