using UnityEngine;

namespace Kun.Tool
{
	public class JsonPrefs<T>
	{
		public JsonPrefs (string key, T defaultValue)
		{
			this.key = key;

			CurrentValue = GetValue (key, defaultValue);

			cacheJson = JsonUtility.ToJson (CurrentValue);

            if (PlayerPrefs.HasKey (key))
			{
				string jsonValue = PlayerPrefs.GetString (key);

				CurrentValue = JsonUtility.FromJson<T> (jsonValue);

				cacheJson = jsonValue;
			}
			else
			{
				CurrentValue = defaultValue;

				cacheJson = JsonUtility.ToJson (defaultValue);
			}
		}

		public static T GetValue (string key, T defaultValue)
		{
			if (PlayerPrefs.HasKey (key))
			{
				string jsonValue = PlayerPrefs.GetString (key);
				return JsonUtility.FromJson<T> (jsonValue);
			}
			else
			{
				return defaultValue;
			}
        }

        /// <summary>
        /// 直接傳入新的obj
        /// </summary>
        /// <param name="newValue">New value.</param>
        public virtual void SetValue (T newValue)
		{
			CurrentValue = newValue;

			Save ();
        }

		/// <summary>
		/// 當绑定的obj有變化時
		/// </summary>
		public virtual void Save ()
		{
			string jsonValue = JsonUtility.ToJson (CurrentValue);

            PlayerPrefs.SetString (key, jsonValue);

			cacheJson = jsonValue;
		}

		protected string key;

		public T CurrentValue { get; private set; }

		protected string cacheJson;
	}
}