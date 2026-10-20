using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    [Serializable]
	public abstract class JsonBase<T> where T:JsonBase<T>
	{
		public JsonBase (Dictionary<string,object> jsonNode)
		{
			this.jsonNode = jsonNode;
		}

		protected string Parse_String (string key)
		{
			object obj;

			if (TryGetValue (key, out obj))
			{
				return obj.ToString ();
			}

			return "";
		}

		protected int Parse_Int (string key)
		{
			object obj;

			int i = 0;

			if (TryGetValue (key, out obj))
			{
				if (!int.TryParse (obj.ToString (), out i))
				{
					Debug.LogError ($"parse fail, key -> {obj}");
				}
			}

			return i;
		}

		protected float Parse_Float (string key)
		{
			object obj;

			float f = 0f;

			if (TryGetValue (key, out obj))
			{
				if (!float.TryParse (obj.ToString (), out f))
				{
					Debug.LogError ($"parse fail, key -> {obj}");
				}
			}

			return f;
		}

		protected bool Parse_Bool (string key)
		{
			object obj;

			bool b = false;

			if (TryGetValue (key, out obj))
			{
				if (!bool.TryParse (obj.ToString (), out b))
				{
					Debug.LogError ($"parse fail, key -> {obj}");
				}
			}

			return b;
		}

		protected bool TryGetValue (string key, out object value)
		{
			bool result = false;

			if (!jsonNode.TryGetValue (key, out value)) 
			{
				Debug.LogError ($"get item fail, key -> {key}");
				value = null;
			}

			return result;
		}

		Dictionary<string,object> jsonNode = null;

		public abstract Dictionary<string,object> CreateJsonNode ();
	}
}
