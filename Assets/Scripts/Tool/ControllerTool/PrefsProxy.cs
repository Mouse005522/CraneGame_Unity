using UnityEngine;

namespace Kun.Tool
{
    public class PrefsProxyBool : PrefsProxy<bool> 
	{
		public PrefsProxyBool (string key) : base (key)
		{

		}

		protected override void SetValueToCache (bool value)
        {
			int intValue = value ? 1 : 0;
			PlayerPrefs.SetInt (key, intValue);
        }

        protected override bool GetCacheValue ()
        {
			var boolValue = PlayerPrefs.GetInt (key) == 1 ? true : false;

			return boolValue;
		}
    }

	public abstract class PrefsProxy<T>
	{
		public PrefsProxy (string key)
		{
			this.key = key;
			value = GetCacheValue ();
		}

		protected abstract T GetCacheValue ();

		protected abstract void SetValueToCache (T value);

		public T Value
		{
			get
			{
				return value;
			}

			set
			{
				this.value = value;
				SetValueToCache (value);
			}
		}

		T value;
		protected string key;
	}
}