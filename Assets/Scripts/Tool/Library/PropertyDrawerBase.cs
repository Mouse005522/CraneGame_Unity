#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Kun.Tool
{
    public class PropertyDrawerBase<T> : PropertyDrawer where T:PropertyAttribute
	{
		protected virtual bool NeedDrawBaseGUI 
		{
			get
			{
				return false; 
			}
		}

		protected T runtimeScript;

		bool init = false;

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			if (NeedDrawBaseGUI) 
			{
				base.OnGUI (position, property, label);
			}

			if (!init) 
			{
				OnEnable ();
				init = true;
			}
		}

		protected virtual void OnEnable ()
		{
			runtimeScript = (T)attribute;
			Debug.Log (runtimeScript.GetType ());
		}
	}
}
#endif