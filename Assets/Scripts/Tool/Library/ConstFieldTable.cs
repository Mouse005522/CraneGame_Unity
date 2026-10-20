using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kun.Tool
{
    /// <summary>
    /// 把常數腳本裡的變數反射出來做的緩存
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public static class ConstFieldTable<TType, TValue>
    {
        const BindingFlags flags = (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        /// <summary>
        /// 靜態建構會依據T做出區隔
        /// 保證一個T只會建構一次
        /// </summary>
        static ConstFieldTable ()
        {
            Type valueType = typeof (TValue);

            var fields = typeof (TType).GetFields (flags).ToList ();
            
            fields.ForEach (f =>
            {
                if (f.FieldType == valueType)
                {
                    keys.Add (f.Name);

                    var value = (TValue)f.GetValue (null);
                    values.Add (value);

                    table.Add ((f.Name, value));
                }
            });
        }

        public static List<string> keys = new List<string> ();
        public static List<TValue> values = new List<TValue> ();
        public static List<(string key, TValue value)> table = new List<(string key, TValue value)> ();
    }
}