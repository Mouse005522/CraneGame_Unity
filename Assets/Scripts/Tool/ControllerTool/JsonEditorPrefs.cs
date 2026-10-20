#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// 當值有變更時會自動觸發callback
    /// </summary>
    public class JsonEditorPrefsController<T> : JsonEditorPrefs<T>
	{
		public JsonEditorPrefsController (string key, T defaultValue, Action<T> onSetValueCallback = null) : base (key, defaultValue)
		{
			if (onSetValueCallback != null)
			{
				onSetValueCallback.Invoke (CurrentValue);
			}

			this.onSetValueCallback = onSetValueCallback;
		}

		Action<T> onSetValueCallback;

		/// <summary>
		/// 直接傳入新的obj
		/// </summary>
		/// <param name="newValue">New value.</param>
		public override void SetValue (T newValue)
		{
			base.SetValue (newValue);

			if (onSetValueCallback != null)
			{
				onSetValueCallback.Invoke (CurrentValue);
			}
		}

		/// <summary>
		/// 當绑定的obj有變化時
		/// </summary>
		public override void SetModify ()
		{
			base.SetModify ();
		}
	}

	public class JsonEditorPrefs<T>
	{
		public JsonEditorPrefs (string key, T defaultValue)
		{
			this.key = key;

			if (EditorPrefs.HasKey (key))
			{
				string jsonValue = EditorPrefs.GetString (key);

				CurrentValue = JsonUtility.FromJson<T> (jsonValue);

				cacheJson = jsonValue;
			}
			else
			{
				CurrentValue = defaultValue;

				cacheJson = JsonUtility.ToJson (defaultValue);
			}

		}

		/// <summary>
		/// 直接傳入新的obj
		/// </summary>
		/// <param name="newValue">New value.</param>
		public virtual void SetValue (T newValue)
		{
			CurrentValue = newValue;

			string jsonValue = JsonUtility.ToJson (newValue);

			EditorPrefs.SetString (key, jsonValue);

			cacheJson = JsonUtility.ToJson (newValue);
		}

		/// <summary>
		/// 當绑定的obj有變化時
		/// </summary>
		public virtual void SetModify ()
		{
			string jsonValue = JsonUtility.ToJson (CurrentValue);

			EditorPrefs.SetString (key, jsonValue);

			cacheJson = jsonValue;
		}

		protected string key;

		public T CurrentValue { get; private set; }

		protected string cacheJson;
	}
}
#endif