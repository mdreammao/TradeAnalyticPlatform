using BackTestingPlatform.Model.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingPlatformTest.Tests
{
 
    public struct Tick1
    {
        public string code { get; set; }

        public Position[] ask { get; set; }
        public List<Position> bid { get; set; }


    }

    class EntityFlattenAndDeflattenPropsTests
    {
        DataRow dr;
        public List<String> propNames = new List<String>();

        public void t2(Type t, string prefix)
        {
            if (IsIListType(t))
            {

            }
            var properties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {

                Type subType = getElementTypeOfIListType(prop.PropertyType);
                t2(subType, String.Concat(prefix, '.', t));

                propNames.Add(prop.Name);
            }

        }

        public static bool IsPropertyIEnumerable(PropertyInfo property)
        {
            return property.PropertyType.GetInterface(typeof(IEnumerable<>).FullName) != null;
        }
        public static bool IsIListType(Type t)
        {
            return t.GetInterface(typeof(IList<>).FullName) != null;
        }

        public static Type getElementTypeOfIListType(Type anIListType)
        {
            if (anIListType.IsArray)
            {
                //a T[]
                return anIListType.GetElementType();
            }
            else
            {
                //a List<T>
                var types = anIListType.GetGenericArguments();
                return types == null ? null : types[0];
            }
        }

        public void testEntityFlattenAndDeflattenProps()
        {
            var t1 = new Tick1
            {
                code = "11111",
                ask = new Position[]
                {
                    new Position(5.26,2222),
                    new Position(5.25,666),
                    new Position(5.23,442)
                },
                bid = new List<Position>
                {
                    new Position(5.29,75),
                    new Position(5.31,612),
                    new Position(5.32,4142)
                }
            };

            BBB bbb = new BBB();
            //bbb.t2(t1,"");
            var properties = typeof(Tick).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var xxxxx = bbb.propNames;
            Object v;
            IEnumerable ve;

            foreach (var prop in properties)
            {
                v = prop.GetValue(t1);
                if (v.GetType() is IList)
                {
                    IList vl = (IList)v;
                    for (int i = 0; i < vl.Count; i++)
                    {
                        var ss = prop.Name + "[" + i + "]";
                    }
                    foreach (var x in (IList)v)
                    {

                    }

                }
            }


        }
    }
}
