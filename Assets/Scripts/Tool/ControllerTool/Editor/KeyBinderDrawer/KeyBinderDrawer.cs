using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kun.Tool
{
#if UNITY_EDITOR
    public abstract class KeyBinderDrawer : PropertyDrawer
	{
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			EditorTool.DrawInProperty (position, property, label, () =>
				{
					string fieldName = property.name;

					// Struct 可以直接給值 不用管refence
					Rect fieldNameRect;
					Rect fieldValueRect;

					string originKey = GetString (property);

					fieldNameRect = fieldValueRect = position;

					EditorGUI.LabelField (fieldNameRect, fieldName);

					//字太多會撞到, 依據對方數量做出偏差
					fieldValueRect.center += new Vector2 (6f * fieldName.Length + 20, 0);

					string showValue = originKey;

					if (HasComment)
					{
						int keyIndex = AllItemKeys.IndexOf (originKey);

						if (keyIndex >= 0)
						{
							string comment = AllComments [keyIndex];
							showValue += ("  ->  " + comment);
						}
					}

					EditorGUI.LabelField (fieldValueRect, showValue);

					Rect infoRect = fieldNameRect;

					infoRect.center += Vector2.up * (LineHeight + LineSpaceHeight);

					UpdateKeyPropertyStatus (originKey, AllItemKeys);

					DrawItemKeyInfo (originKey, AllItemKeys, infoRect);

					Rect btnRect = infoRect;

					//按鈕自帶行至中, 會被往下拉, 所以往上浮一點
					btnRect.center += new Vector2 (310, -5);

					btnRect.width = BtnWidth;
					btnRect.height = BtnHeight;

					bool clickSwitchKey = false;

					clickSwitchKey = GUI.Button (btnRect, $"修改 \n {ItemKeyShowName}");

					if (clickSwitchKey)
					{
						string [] comments = HasComment ? AllComments.ToArray () : null;

						Pop_UpSelectWindow.ShowWindow ((newKey) =>
							{
								SetString (newKey, property);
								property.serializedObject.ApplyModifiedProperties ();
							}, originKey, AllItemKeys.ToArray (), comments);
					}

				});
		}

		public const float BtnWidth = 150f;

		public const float BtnHeight = 40f;

		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			//搜尋框的下移
			return base.GetPropertyHeight (property, label) + (LineHeight + LineSpaceHeight) + BtnHeight;
		}

		float GetAbs (string fieldName)
		{
			int fieldNameLength = fieldName.Length;

			//先看有幾個字
			float length = fieldNameLength * 8f;

			//再加上一個固定長度
			length += 50;

			return length;
		}

		public const float LineHeight = 16f;

		public const float LineSpaceHeight = 4f;

		protected KeyPropertyStatus KeyPropertyStatus{ get; private set;}

		void UpdateKeyPropertyStatus (string itemKey, List<string> allItemKeys)
		{
			Color drawColor = Color.red;

			string info = "";

			if (string.IsNullOrEmpty (itemKey))
			{
				KeyPropertyStatus = KeyPropertyStatus.Empty;
			}
			else if (allItemKeys.Contains (itemKey) == false)
			{
				KeyPropertyStatus = KeyPropertyStatus.NotExistKey;
			}
			else
			{
				KeyPropertyStatus = KeyPropertyStatus.ExistKey;
			}
		}

		void DrawItemKeyInfo (string itemKey, List<string> allItemKeys, Rect infoRect)
		{
			Color drawColor = Color.red;

			string info = "";

			if (KeyPropertyStatus == KeyPropertyStatus.Empty) 
			{
				drawColor = Color.yellow;
				info = "尚未綁定key";
			}
			else if (KeyPropertyStatus == KeyPropertyStatus.NotExistKey)
			{
				drawColor = Color.red;
				info = "此key不存在";
			}

			EditorTool.DrawInColor (drawColor, () =>
				{
					GUI.Label (infoRect, info);
				});
		}

		protected virtual List<string> AllItemKeys { get; }

		/// <summary>
		/// key是否有附帶的註解
		/// </summary>
		/// <value><c>true</c> if this instance has comment; otherwise, <c>false</c>.</value>
		protected virtual bool HasComment
		{
			get
			{
				return false;
			}
		}

		protected virtual List<string> AllComments { get; }

		public abstract string ItemKeyShowName { get; }

		protected virtual string GetString (SerializedProperty property)
		{
			return property.stringValue;
		}

		protected virtual void SetString (string newStr, SerializedProperty property)
		{
			property.stringValue = newStr;
		}
	}

	public enum KeyPropertyStatus
	{
		/// <summary>
		/// 空字串
		/// </summary>
		Empty,
		/// <summary>
		/// 有值, 此key不存在在列表中
		/// </summary>
		NotExistKey,
		/// <summary>
		/// 存在的Key
		/// </summary>
		ExistKey
	}
	#endif
}