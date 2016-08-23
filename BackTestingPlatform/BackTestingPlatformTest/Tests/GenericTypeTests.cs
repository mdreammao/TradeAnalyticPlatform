using BackTestingPlatform.Model.Option;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
    public class GenericTypeTests
    {
        public static void test1()
        {
            var x=new OptionInfo();
            var properties = typeof(OptionInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            object boxed = new OptionInfo();
            var prop1 = typeof(OptionInfo).GetProperty("optionCode");
            prop1.SetValue(boxed, "xxxxx");
            OptionInfo x1 = (OptionInfo)boxed;
            var y = new OptionDaily();
            var prop21 = typeof(OptionDaily).GetProperty("open");
            prop21.SetValue(y, 232.333);
        }
    }
}
