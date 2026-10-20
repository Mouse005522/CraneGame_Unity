using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kun.Tool
{
    public static class EnumTool
    {
        public static string FlagsToString<T> (this T flags)  where T:Enum, IConvertible
        {
            var flagValue = Convert.ToInt32 (flags);

            var containFlags = GetIter<T> ().FindAll (t => 
            {
                var target = Convert.ToInt32 (t);
                bool isContain = (flagValue & target) == target;
                return isContain;
            });

            var msgs = string.Join (", ", containFlags);

            return msgs;
        }

        /// <summary>
        /// 檢測flags是否包含
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CheckContaions<T> (this T src, T target)  where T:Enum, IConvertible
        {
            var srcValue = Convert.ToInt32 (src);
            var targetValue = Convert.ToInt32 (target);

            return (srcValue & targetValue) == targetValue;
        }

        /// <summary>
        /// 檢查是不是最後一個enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static bool CheckIsLast<T> (this T enumValue) where T : Enum
        {
            var values = Enum.GetValues (typeof (T));

            int value = Convert.ToInt32 (enumValue);

            return value >= (values.Length - 1);
        }

        /// <summary>
        /// enum往後移
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        public static T MoveToNext<T> (this T enumValue) where T : Enum
        {
            int value = Convert.ToInt32 (enumValue);

            int nextValue = value + 1;

            return (T)Enum.ToObject (typeof (T), nextValue);
        }

        /// <summary>
        /// 是否是上一個
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        public static bool CheckIsPrev<T> (this T enumValue, T checkTarget) where T : Enum
        {
            int value = Convert.ToInt32 (enumValue);

            int checkValue = Convert.ToInt32 (checkTarget);

            return value == (checkValue - 1);
        }

        /// <summary>
        /// 是否已經經過了
        /// 這個是>= -1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        public static bool CheckIsPass<T> (this T enumValue, T checkTarget) where T : Enum
        {
            int value = Convert.ToInt32 (enumValue);

            int checkValue = Convert.ToInt32 (checkTarget);

            return value >= (checkValue - 1);
        }

        public static T GetEnumValue<T> (this int value) where T : Enum
        {
            return (T)Enum.ToObject (typeof (T), value);
        }

        public static List<T> GetIter<T> ()
        {
            List<T> iter = new List<T> ((T[])Enum.GetValues (typeof (T)));

            Type enumType = typeof (T);

            iter.RemoveAll (item =>
            {
                FieldInfo fieldInfo = enumType.GetField (item.ToString ());

                IgnoreIter ingnoreIter = fieldInfo.GetCustomAttribute<IgnoreIter> ();

                return ingnoreIter != null;
            });

            return iter;
        }

        /// <summary>
		/// 有attr顯示attr不然就顯示 enum的名稱
		/// </summary>
		/// <returns>The show msgs.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static string[] GetShowMsgs<T> ()
        {
            Type enumType = typeof (T);

            List<string> msgs = new List<string> ();

            Array.ForEach ((T[])Enum.GetValues (typeof (T)), item =>
            {
                //enum 的變數名稱 就是 ToString
                string fieldName = item.ToString ();

                FieldInfo fieldInfo = enumType.GetField (fieldName);

                EnumSummary enumSummary = fieldInfo.GetCustomAttribute<EnumSummary> ();

                string msg;

                if (enumSummary != null)
                {
                    msg = enumSummary.summary;
                }
                else
                {
                    msg = fieldName;
                }

                msgs.Add (msg);
            });

            return msgs.ToArray ();
        }
    }

    /// <summary>
	/// Enum.GetIter 不被列入Iter
	/// </summary>
	public class IgnoreIter : Attribute
    {

    }

    /// <summary>
    /// 把Enum 轉成 String[] 時候顯示的文字
    /// </summary>
    public class EnumSummary : Attribute
    {
        public EnumSummary (string summary)
        {
            this.summary = summary;
        }

        public string summary;
    }

    /// <summary>
    /// 依指定 Attribute 過濾 enum 值的泛型快取工具
    /// 透過靜態建構式在首次存取時建立快取
    /// </summary>
    public static class EnumFilterHelper<TEnum, TAttribute>
        where TEnum : Enum
        where TAttribute : Attribute
    {
        /// <summary>
        /// 帶有指定 Attribute 的 enum 值列表
        /// </summary>
        public static readonly List<TEnum> FilteredValues;

        /// <summary>
        /// 帶有指定 Attribute 的 enum int 值陣列
        /// </summary>
        public static readonly int[] FilteredIntValues;

        /// <summary>
        /// 帶有指定 Attribute 的 enum 對應 EnumMsg 訊息陣列
        /// </summary>
        public static readonly string[] FilteredMsgs;

        /// <summary>
        /// 帶有指定 Attribute 的 enum 值在原始 enum 中的 index 陣列
        /// </summary>
        public static readonly int[] FilteredEnumIndices;

        static EnumFilterHelper ()
        {
            Type enumType = typeof (TEnum);
            TEnum[] allValues = (TEnum[]) Enum.GetValues (enumType);

            var filtered = new List<TEnum> ();
            var filteredInts = new List<int> ();
            var filteredMsgs = new List<string> ();
            var filteredIndices = new List<int> ();

            for (int i = 0; i < allValues.Length; i++)
            {
                TEnum value = allValues[i];
                FieldInfo fieldInfo = enumType.GetField (value.ToString ());

                if (fieldInfo.GetCustomAttribute<TAttribute> () != null)
                {
                    filtered.Add (value);
                    filteredInts.Add (Convert.ToInt32 (value));
                    filteredIndices.Add (i);

                    EnumMsg enumMsg = fieldInfo.GetCustomAttribute<EnumMsg> ();
                    filteredMsgs.Add (enumMsg != null ? enumMsg.msg : value.ToString ());
                }
            }

            FilteredValues = filtered;
            FilteredIntValues = filteredInts.ToArray ();
            FilteredMsgs = filteredMsgs.ToArray ();
            FilteredEnumIndices = filteredIndices.ToArray ();
        }
    }
}
