using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kun.Tool
{
    public static class MsgTool
    {
        public static string SecondsToMinutesString (int seconds)
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }

        /// <summary>
        /// 產生 帶顏色的字串
        /// </summary>
        /// <returns>The string color.</returns>
        /// <param name="inputStr">Input string.</param>
        /// <param name="inputColor">Input color.</param>
        public static string MixStringColor (object input, Color inputColor)
        {
			string inputStr = input.ToString ();

            string left = ColorUtility.ToHtmlStringRGB (inputColor).ToLower ();

            return string.Format ("<color=#{0}>{1}</color>", left, inputStr);
        }

        /// <summary>
        /// 產生 帶顏色的字串
        /// </summary>
        /// <returns>The string color.</returns>
        /// <param name="inputStr">Input string.</param>
        /// <param name="inputColor">Input color.</param>
        public static string MixStringColor (string inputStr, Color inputColor)
		{
			string left = ColorUtility.ToHtmlStringRGB (inputColor).ToLower ();

			return string.Format ("<color=#{0}>{1}</color>", left, inputStr);
		}

		public static string GetFullMessage (this Exception e)
		{
			string excetptionType = e.GetType ().Name;
			string stackTrace = e.StackTrace;

			return $"Type -> {excetptionType}, stackTrace -> {stackTrace}";
		}

		/// <summary>
		/// 把集合混和成具有分行號的單一字串
		/// </summary>
		/// <returns>The multiply lines.</returns>
		/// <param name="datas">Datas.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static string MergeMultiplyLines<T> (List<T> datas)
		{
			var lines = datas.ConvertAll (data => JsonUtility.ToJson (data));

			return MergeMultiplyLines (lines);
		}

		/// <summary>
		/// 把集合混和成具有分行號的單一字串
		/// </summary>
		/// <returns>The multiply lines.</returns>
		/// <param name="lines">Lines.</param>
		public static string MergeMultiplyLines (List<string> lines)
		{
			StringBuilder builder = new StringBuilder ();

			lines.ForEach (line => builder.AppendLine (line));

			return builder.ToString ();
		}

		/// <summary>
		/// 不斷加上後綴, 直到出現沒用過的為止
		/// </summary>
		/// <param name="baseName"></param>
		/// <param name="superimposedMark"></param>
		/// <param name="oldKeys"></param>
		/// <returns></returns>
		public static string GetNewStateName (string baseName, string superimposedMark, List<string> oldKeys)
		{
			string currentName = baseName;

			while (true)
			{
				if (oldKeys.Contains (currentName) == false)
				{
					break;
				}
				else
				{
					currentName += superimposedMark;
				}
			}

			return currentName;
		}
	}
}
