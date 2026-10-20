using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kun.Tool
{
	#if UNITY_EDITOR
	public abstract class KeyBinderDrawer_Int : KeyBinderDrawer
	{
		protected override string GetString (SerializedProperty property)
		{
			return property.intValue.ToString ();
		}

		protected override void SetString (string newStr, SerializedProperty property)
		{
			int value;

			if (int.TryParse (newStr, out value) == false)
			{
				Debug.LogError ("can't parse, " + newStr);
			}

			property.intValue = value;
		}
	}
	#endif
}