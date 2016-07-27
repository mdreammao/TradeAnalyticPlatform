using Autofac;
using BackTestingPlatform.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static IContainer container;
        public static IDictionary<string, object> parameters;

        /// <summary>
        /// 应用的全局初始化
        /// </summary>
        public static void Initialize()
        {
            //初始化Autofac核心 - container
            ContainerBuilder builder = new ContainerBuilder();

            //Autofac中注册所有组件
            _RegisterComponents(builder);
            container = builder.Build();

            //初始化parameters
            parameters = new Dictionary<string, object>();


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

            cb.RegisterInstance(new TradeDaysInfoRepositoryFromWind());
        }
    }


}
