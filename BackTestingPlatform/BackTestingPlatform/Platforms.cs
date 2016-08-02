using Autofac;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;

namespace BackTestingPlatform.Core
{
    /// <summary>
    /// 提供一个全局可访问的类，存放各种重要变量
    /// </summary>
    public class Platforms
    {
        //Autofac容器
        public static IContainer container;
        //全局基础数据的变量字典
        public static IDictionary<string, object> BasicInfo;
        //重要变量：交易日
        public static List<int> tradeDays;

        /// <summary>
        /// 整个应用的全局初始化
        /// </summary>
        public static void Initialize()
        {
            //初始化Autofac核心 - container
            ContainerBuilder builder = new ContainerBuilder();

            //Autofac中注册所有组件
            _RegisterComponents(builder);
            container = builder.Build();

            //初始化parameters
            BasicInfo = new Dictionary<string, object>();

            //初始化交易日数据
            readTradeDaysFromLocalFile();
        }
        /// <summary>
        /// 整个应用的终止
        /// </summary>
        public static void ShutDown()
        {
            if (_windAPI != null)
            {
                if (_windAPI.isconnected())
                {
                    _windAPI.stop();
                }
            }
        }


        private static WindAPI _windAPI;
        /// <summary>
        /// 获取可立即使用的WindAPI,如果处于未连接状态自动开启
        /// </summary>
        /// <returns></returns>
        public static WindAPI GetWindAPI()
        {
            if (_windAPI == null)
            {
                _windAPI = new WindAPI();
            }
            if (!_windAPI.isconnected())
            {
                _windAPI.start();
            }
            return _windAPI;
        }

        /// <summary>
        /// 为container注册各种接口
        /// 具体参见： https://autofac.org/
        /// </summary>
        /// <param name="builder"></param>
        private static void _RegisterComponents(ContainerBuilder cb)
        {
            cb.RegisterInstance(new KLinesDataRepositoryFromWind()).As<KLinesDataRepository>();

            cb.RegisterInstance(new ASharesInfoRepositoryFromWind()).As<ASharesInfoRepository>();//*****测试部分*****

            cb.RegisterInstance(new TradeDaysInfoRepositoryFromWind());

            var asm = Assembly.GetExecutingAssembly();

            //自动扫描注册
            /**/
            cb.RegisterAssemblyTypes(asm)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces();
           
        }

        static void readTradeDaysFromLocalFile()
        {
            var path = System.Environment.CurrentDirectory;
            if (path.EndsWith("bin\\Debug")) path = path.Substring(0, path.Length - 10);
            path += @"\RESOURCES\trade_days_2010_2016.txt";
            tradeDays=new List<int>(2000);
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        tradeDays.Add(Convert.ToInt32(line));                        
                    }

                    Platforms.BasicInfo.Add("tradeDays", tradeDays);
                }
            }catch(FileNotFoundException e)
            {
                Console.WriteLine(e);
            }
            
        }
    }


}
