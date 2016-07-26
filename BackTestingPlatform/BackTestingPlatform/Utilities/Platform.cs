using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WAPIWrapperCSharp;
namespace BackTestingPlatform.Utilities
{
    class MyPlatform
    {
        static MyPlatformContext _currentContext;
        public static MyPlatformContext currentContext()
        {
            return _currentContext;
        }
        public static int init()
        {
            _currentContext = new MyPlatformContext();
            _currentContext.getWindAPI();            
            return 0;
        }
    }

    class MyPlatformContext
    {
        WindAPI windAPI;
        public WindAPI getWindAPI()
        {
            if (windAPI == null)
            {
                windAPI = new WindAPI();
            }
            if (windAPI.isconnected())
            {
                windAPI.start();
            }
            return windAPI;
        }
        

    }
  
}
