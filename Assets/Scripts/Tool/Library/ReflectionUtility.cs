using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public static class ReflectionUtility
	{
		public static object GetValue (object target, string memberName)
		{
			Type type = target.GetType ();
			
			var findCache = FindTypeCache (type);

			return findCache.GetValue (target, memberName);
		}

		public static void SetValue (object target, string memberName, object value)
		{
			Type type = target.GetType ();
			
			var findCache = FindTypeCache (type);

			findCache.SetValue (target, memberName, value);
		}

		static TypeReflectionCache FindTypeCache (Type type)
		{
			var findCache = typeReflectionCaches.Find (cache => cache.Type == type);

			if (findCache == null) 
			{
				findCache = new TypeReflectionCache (type);
				typeReflectionCaches.Add (findCache);
			}

			return findCache;
		}

		static List<TypeReflectionCache> typeReflectionCaches = new List<TypeReflectionCache> ();

		
		class TypeReflectionCache
		{
			public object GetValue (object target, string memberName)
			{
				var findCache = caches.Find (cache => cache.memberName == memberName);

				if (findCache != null) 
				{
					return findCache.GetValue.Invoke (target);
				}
				else
				{
					Debug.LogError ($"can't find member , type -> {target.GetType()}, memberName -> {memberName}");

					return null;
				}
			}
			
			public void SetValue (object target, string memberName, object value)
			{
				var findCache = caches.Find (cache => cache.memberName == memberName);

				if (findCache != null) 
				{
					findCache.SetValue.Invoke (target, value);
				}
				else
				{
					Debug.LogError ($"can't find member , type -> {target.GetType()}, memberName -> {memberName}");
				}
			}

			public Type Type{ get; private set;}

			public TypeReflectionCache (Type type)
			{
				this.Type = type;

				caches = new List<GetSetCache> ();

				var fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

				type.GetFields (fieldFlags).ToList ().ForEach (fieldInfo=>
					{
						var memberName = fieldInfo.Name;

						Func<object,object> GetValue = (target)=>
						{
							return fieldInfo.GetValue (target);
						};

						Action<object,object> SetValue = (target,value)=>
						{
							fieldInfo.SetValue (target,value);
						};

						GetSetCache cache = new GetSetCache (memberName, GetValue, SetValue);

						caches.Add (cache);
					});

				type.GetProperties (fieldFlags).ToList ().ForEach (propertyInfo=>
					{
						var memberName = propertyInfo.Name;

						Func<object,object> GetValue = (target)=>
						{
							return propertyInfo.GetValue (target);
						};

						Action<object,object> SetValue = (target,value)=>
						{
							propertyInfo.SetValue (target,value);
						};

						GetSetCache cache = new GetSetCache (memberName, GetValue, SetValue);

						caches.Add (cache);
					});
			}

			List<GetSetCache> caches = new List<GetSetCache> ();

			/// <summary>
			/// 由於FieldInfo跟PropertyInfo的基礎型別MemberInfo不具有Get, Set的功能, Get Set是各自完成, 所以通過RunTime產生Callback
			/// </summary>
			class GetSetCache
			{
				public GetSetCache (string memberName, Func<object,object> GetValue, Action<object,object> SetValue)
				{
					this.memberName = memberName;
					this.GetValue = GetValue;
					this.SetValue = SetValue;
				}
				
				public string memberName;

				public Func<object,object> GetValue;

				public Action<object,object> SetValue;
			}
		}
	}
}