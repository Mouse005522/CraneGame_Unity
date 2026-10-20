using UnityEngine;
using System;

namespace Kun.Tool
{
    public static class PlayerPrefsUtility
    {
        static PlayerPrefsUtility ()
        {
            Prefix = Application.dataPath;
        }

        /// <summary>
        /// 用路徑當前綴才可以同專案不同資夾錯開
        /// </summary>
        static string Prefix = "";

        public static string GetString (string key, string defaultValue)
        {
            var fullKey = GetFullKey (key);

            var stringValue = PlayerPrefs.GetString (fullKey, defaultValue);

            return stringValue;
        }

        public static void SetString (string key, string stringValue)
        {
            var fullKey = GetFullKey (key);

            PlayerPrefs.SetString (fullKey, stringValue);
        }

        public static T GetEnum<T> (string key) where T : Enum
        {
            var fullKey = GetFullKey (key);

            var defaultValue = Convert.ToInt32 (default (T));
            var intValue = PlayerPrefs.GetInt (fullKey, defaultValue);

            return (T)Enum.ToObject (typeof (T), intValue);
        }

        public static void SetEnum<T> (string key, T value) where T : Enum
        {
            var fullKey = GetFullKey (key);

            var intValue = Convert.ToInt32 (value);
            PlayerPrefs.SetInt (fullKey, intValue);
        }

        static string GetFullKey (string key)
        {
            return Prefix + key;
        }
    }
}
