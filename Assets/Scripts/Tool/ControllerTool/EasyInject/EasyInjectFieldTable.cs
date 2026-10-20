using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kun.Tool
{
    public static class EasyInjectFieldTable<T> where T : Attribute
    {
        static ConcurrentDictionary<Type, List<MemberSetter>> fieldTables = new ConcurrentDictionary<Type, List<MemberSetter>> ();

        /// <summary>
        /// 找出綁定有某個Attribute的Field
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<MemberSetter> GetSetters (Type type)
        {
            List<MemberSetter> fields = fieldTables.GetOrAdd (type, (_type) => 
            {
                var setters = CreateSetters (_type);
                return setters;
            });

            return fields;
        }

        static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        static List<MemberSetter> CreateSetters (Type type) 
        {
            List<MemberSetter> setters = new List<MemberSetter> ();
            var typeStack = GetTypeStack (type);

            List<FieldInfo> fields = typeStack.SelectMany (type => type.GetFields (flags))
                .GroupBy (f => f.Name).Select (g => g.First ()).ToList ();

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute (typeof (T)) != null)
                {
                    MemberSetter memberSetter = new MemberSetter (field);

                    setters.Add (memberSetter);
                }
            }

            //同一個property在子型別與父型別的參考不同
            //如果是protected Set那就要在子型別才能Set
            //先用name做分組
            //找出CanWrite的參考
            //如果都找不到表示同個Property不能被Set
            List<PropertyInfo> properties = typeStack.SelectMany (type => type.GetProperties (flags))
                .GroupBy (p => p.Name).Select (g => g.FirstOrDefault (p => p.CanWrite)).Where (p => p != null).ToList ();

            foreach (var property in properties) 
            {
                if (property.GetCustomAttribute (typeof (T)) != null)
                {
                    MemberSetter memberSetter = new MemberSetter (property);

                    setters.Add (memberSetter);
                }
            }

            return setters;
        }

        /// <summary>
        /// 找出包含自己在內所有子型別
        /// </summary>
        /// <param name="curType"></param>
        /// <returns></returns>
        static List<Type> GetTypeStack (Type curType) 
        {
            List<Type> typeStack = new List<Type> ();

            while (curType != null)
            {
                typeStack.Add (curType);
                curType = curType.BaseType;
            }

            return typeStack;
        }
    }

    /// <summary>
    /// 對Field與Property進行包裝
    /// </summary>
    public class MemberSetter
    {
        FieldInfo field;
        PropertyInfo property;
        bool isProperty;

        public Type MemberType { get; private set; }

        Action<object, object> setter;

        public void SetValue (object target, object value)
        {
            try
            {
                setter.Invoke (target, value);
            }
            catch (Exception e) 
            {
                LoggerRouter.Exception (e);
            }
        }

        public MemberSetter (FieldInfo field)
        {
            this.field = field;
            this.isProperty = false;
            this.MemberType = field.FieldType;

            setter = field.SetValue;
        }

        public MemberSetter (PropertyInfo property)
        {
            this.property = property;
            this.isProperty = true;
            this.MemberType = property.PropertyType;

            setter = property.SetValue;
        }
    }
}
