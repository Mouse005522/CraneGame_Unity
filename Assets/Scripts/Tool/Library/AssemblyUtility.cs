using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kun.Tool
{
    public static class AssemblyUtility
    {
        static AssemblyUtility () 
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            List<Assembly> assemblies = currentDomain.GetAssemblies ().ToList ();

            allTypes = assemblies.SelectMany (assembly => assembly.GetTypes ()).ToList ();

            allTypes.ForEach (type=> 
            {
                //find也只會找第一個, 所以相同的就找一個塞吧
                if (typeTable.ContainsKey (type.Name) == false) 
                {
                    typeTable.Add (type.Name, type);
                }

                if (typeTable_FullName.ContainsKey (type.FullName) == false)
                {
                    typeTable_FullName.Add (type.FullName, type);
                }
            });
        }

        //const string UnityRuntimeAssemblyName = "Assembly-CSharp";

        static List<Type> allTypes;

        static Dictionary<string, Type> typeTable = new Dictionary<string, Type> ();

        static Dictionary<string, Type> typeTable_FullName = new Dictionary<string, Type> ();

        public static Type GetType (string typeName) 
        {
            if (typeTable.TryGetValue (typeName, out Type findType)) 
            {
                return findType;
            }

            if (typeTable_FullName.TryGetValue (typeName, out findType))
            {
                return findType;
            }

            return null;
        }

        /// <summary>
        /// 取得繼承T且不為抽象的所有type
        /// 不為抽象是為了只取最後一層
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetSubclasses<T> () 
        {
            var baseType = typeof (T);
            return GetSubclasses (baseType);
        }

        /// <summary>
        /// 取得繼承T且不為抽象的所有type
        /// 不為抽象是為了只取最後一層
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetSubclasses (Type baseType) 
        {
            return allTypes.FindAll (type=> 
            {
                return type.IsAbstract == false && type.IsSubclassOf (baseType) && type != baseType;
            });
        }
    }
}
