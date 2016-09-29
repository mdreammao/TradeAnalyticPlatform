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

            var list1 = new List<int>() { 1, 2, 3, 4, 5 };
            var list2=list1.Cast<int>().ToList();
            list2[2] = 9;
           
        }
    }
}
