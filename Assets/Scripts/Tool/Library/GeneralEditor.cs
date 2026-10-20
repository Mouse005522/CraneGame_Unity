using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kun.Tool
{
#if UNITY_EDITOR
    public static class GeneralEditor 
	{
		public const float fieldNameWidth = 150f;
		public const float buttonWidth = 100f;
		public const float flatButtonHigh = 20f;

		public const float scrollBarHeight = 500f;

		public static GUIStyle FieldNameGUIStyle => EditorStyles.label;

		/// <summary>
		/// 齒輪狀 icon
		/// </summary>
		/// <value>The content of the switch GUI.</value>
		public static GUIContent SwitchGUIContent => EditorGUIUtility.IconContent ("d__Popup");

		public static GUIContent FocusGUIContent => EditorGUIUtility.IconContent ("gradient_down_swatch");

		/// <summary>
		/// 重新整理 icon
		/// </summary>
		/// <value>The content of the switch GUI.</value>
		public static GUIContent RefreshGUIContent => EditorGUIUtility.IconContent ("d_RotateTool");

		public static GUIContent ArrowGUIContent => EditorGUIUtility.IconContent ("varpin tooltip");

		public static GUIContent NodeBtnGUIContent => EditorGUIUtility.IconContent ("pivotdot");

		public static GUIStyle NodeBoxGUIStyle = new GUIStyle ("flow node 1");

		public static GUIStyle NodeBox_OnGUIStyle = new GUIStyle ("flow node 1 on");

		/// <summary>
		/// 流程上無法抵達的Node
		/// </summary>
		/// <value>The un reach node box GUI style.</value>
		public static GUIStyle UnReachNodeBoxGUIStyle = new GUIStyle ("flow node 0");

		/// <summary>
		/// 流程上無法抵達的Node
		/// </summary>
		/// <value>The un reach node on GUI style.</value>
		public static GUIStyle UnReachNode_OnGUIStyle = new GUIStyle ("flow node 0 on");

		public static GUIStyle EnterNodeBoxGUIStyle = new GUIStyle ("flow node 5");

		public static GUIStyle EnterNode_OnGUIStyle = new GUIStyle ("flow node 5 on");

		public static GUIStyle ExitNodeBoxGUIStyle = new GUIStyle ("flow node 6");

		public static GUIStyle ExitNode_OnGUIStyle = new GUIStyle ("flow node 6 on");


		static GeneralEditor ()
		{
			BoxGUIStyle = new GUIStyle ((GUIStyle)"Badge");
			//BoxGUIStyle.normal.background = MakeTexture (1, 1, new Color (0.3f, 0.3f, 0.3f));

			OrFieldNameGUIStyle = new GUIStyle (EditorStyles.label);
			OrFieldNameGUIStyle.normal.textColor = Color.blue;

			AndFieldNameGUIStyle = new GUIStyle (EditorStyles.label);
			AndFieldNameGUIStyle.normal.textColor = Color.red;

			WorryBtmGUIStyle = new GUIStyle (EditorStyles.toolbarButton);
			WorryBtmGUIStyle.normal.textColor = Color.red;
			WorryBtmGUIStyle.fontStyle = FontStyle.Bold;

			WorryFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);
			WorryFieldNameGUIStyle.normal.textColor = Color.red;
			WorryFieldNameGUIStyle.fontStyle = FontStyle.Bold;

			WorryBigFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);
			WorryBigFieldNameGUIStyle.normal.textColor = Color.red;
			WorryBigFieldNameGUIStyle.fontStyle = FontStyle.Bold;
			WorryBigFieldNameGUIStyle.fontSize = 15;

			WarnFieldNameGUIStyle = new GUIStyle (EditorStyles.label);
			WarnFieldNameGUIStyle.normal.textColor = new Color (0.9f, 0.8588f, 0.455f);

			WarnBigFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);
			WarnBigFieldNameGUIStyle.normal.textColor = new Color (0.9f, 0.8588f, 0.455f);
			WarnBigFieldNameGUIStyle.fontStyle = FontStyle.Bold;
			WarnBigFieldNameGUIStyle.fontSize = 15;

			TapBigFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);
			TapBigFieldNameGUIStyle.normal.textColor = new Color (0.3137f, 0.6212f, 0.8867f);
			TapBigFieldNameGUIStyle.fontStyle = FontStyle.Bold;
			TapBigFieldNameGUIStyle.fontSize = 15;

			OrBoxGUIStyle = new GUIStyle (BoxGUIStyle);
			OrBoxGUIStyle.normal.background = MakeTex (600, 1, new Color (0.8392f, 0.9249f, 1, 1));

			AndBoxGUIStyle = new GUIStyle (BoxGUIStyle);
			AndBoxGUIStyle.normal.background = MakeTex (600, 1, Color.red);

			var _TranslucentBlack = Color.black;
			_TranslucentBlack.a = 0.2f;
			TranslucentBlack = _TranslucentBlack;
		}

		public static GUIStyle OrFieldNameGUIStyle { get; private set; }

		public static GUIStyle AndFieldNameGUIStyle { get; private set; }

		public static GUIStyle WorryBtmGUIStyle { get; private set; }

		public static GUIStyle WorryFieldNameGUIStyle { get; private set; }

		public static GUIStyle WorryBigFieldNameGUIStyle { get; private set; }

		public static GUIStyle WarnFieldNameGUIStyle { get; private set; }

		public static GUIStyle WarnBigFieldNameGUIStyle { get; private set; }

		public static GUIStyle TapBigFieldNameGUIStyle { get; private set; }

		public static GUIStyle TitleNameGUIStyle => EditorStyles.boldLabel;

		//public static GUIStyle FlatButtonGUIStyle => (GUIStyle)"IN EditColliderButton";

		public static GUIStyle RadioButtonGUIStyle => EditorStyles.radioButton;


		public static GUIStyle BoxGUIStyle { get; private set; }

		public static GUIContent UpGUIContent => EditorGUIUtility.IconContent ("ProfilerTimelineRollUpArrow");

		public static GUIContent DownGUIContent => EditorGUIUtility.IconContent ("ProfilerTimelineDigDownArrow");

		public static Color TranslucentBlack { get; private set; }

		public static GUIStyle EffectArea => (GUIStyle)"ShurikenEffectBg";

		public static GUIStyle OrBoxGUIStyle { get; private set; }

		public static GUIStyle AndBoxGUIStyle { get; private set; }

		static Texture2D MakeTex (int width, int height, Color col)
		{
			Color[] pix = new Color[width*height];

			for(int i = 0; i < pix.Length; i++)
				pix[i] = col;

			Texture2D result = new Texture2D(width, height);
			result.SetPixels(pix);
			result.Apply();

			return result;
		}

		public const float areaInterval = 10f;

		public const float buttonHorzInterval = 30f;

		/// <summary>
		/// 只有圖案 沒有底圖的按鈕
		/// </summary>
		/// <returns><c>true</c>, if no background button was drawn, <c>false</c> otherwise.</returns>
		public static bool DrawNoBgButton(GUIContent guiContent)
		{
			EditorGUILayout.LabelField (guiContent, GUILayout.Width(15));

			Rect rect = GUILayoutUtility.GetLastRect ();
			Event currentEvent = Event.current;

			if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
			{
				if (rect.Contains (currentEvent.mousePosition)) 
				{
					currentEvent.Use ();

					if (currentEvent.clickCount >= 2)
					{
						return true;
					}
				}
			}

			return false;
		}

		public static void ModifyRectPosDelta (Vector2 deltaPos, ref Rect rect)
		{
			float processPosX = rect.x + deltaPos.x;
			float processPosY = rect.y + deltaPos.y;

			rect = new Rect (processPosX, processPosY, rect.width, rect.height);
		}

		public static void DrawSerializablePoint(SerializedProperty rootProperty,string fieldName)
		{
			SerializedProperty serializablePointProperty = rootProperty.FindPropertyRelative (fieldName);

			EditorGUI.indentLevel--;
			bool openArea = EditorGUILayout.PropertyField (serializablePointProperty);
			EditorGUI.indentLevel++;

			if (openArea) 
			{
				EditorGUI.indentLevel++;
				DrawSerializableVector3 (serializablePointProperty, "position");
				DrawSerializableVector3 (serializablePointProperty, "rotation");
				EditorGUI.indentLevel--;
			}
		}

		public static void DrawSerializableVector3(SerializedProperty rootProperty,string fieldName)
		{
			SerializedProperty serializableVector3Property = rootProperty.FindPropertyRelative (fieldName);

			EditorGUI.indentLevel--;
			bool openArea = EditorGUILayout.PropertyField (serializableVector3Property);
			EditorGUI.indentLevel++;

			if (openArea) 
			{
				EditorGUI.indentLevel++;
				DrawCustomSerlizedField ("x", serializableVector3Property);
				DrawCustomSerlizedField ("y", serializableVector3Property);
				DrawCustomSerlizedField ("z", serializableVector3Property);
				EditorGUI.indentLevel--;
			}
		}

		public static void DrawSerializableQuaternion(SerializedProperty rootProperty,string fieldName)
		{
			SerializedProperty serializableQuaternionProperty = rootProperty.FindPropertyRelative (fieldName);

			EditorGUI.indentLevel--;
			bool openArea = EditorGUILayout.PropertyField (serializableQuaternionProperty);
			EditorGUI.indentLevel++;

			if (openArea) 
			{
				EditorGUI.indentLevel++;
				DrawCustomSerlizedField ("x", serializableQuaternionProperty);
				DrawCustomSerlizedField ("y", serializableQuaternionProperty);
				DrawCustomSerlizedField ("z", serializableQuaternionProperty);
				DrawCustomSerlizedField ("w", serializableQuaternionProperty);
				EditorGUI.indentLevel--;
			}
		}

        public static void DrawVariableField (string variableName, Action drawAndGetInput, float? overrideFieldWidth = null)
		{
			EditorGUILayout.BeginHorizontal ();
			{
				EditorGUILayout.LabelField (variableName, FieldNameGUIStyle, GUILayout.Width (fieldNameWidth));
				drawAndGetInput.Invoke ();
			}
			EditorGUILayout.EndHorizontal ();
		}

		public static void DrawCustomSerlizedField(string key,SerializedProperty rootProperty)
		{
			EditorGUILayout.BeginHorizontal ();
			{
				string KeyFieldName = key;

				EditorGUILayout.LabelField (KeyFieldName, GUILayout.Width (200));

				SerializedProperty findProperty = rootProperty.FindPropertyRelative (KeyFieldName);

				EditorGUILayout.PropertyField (findProperty, new GUIContent (""));
			}
			EditorGUILayout.EndHorizontal ();
		}

		const float BtnSize = 24f;

		public static UpDownModuleStatus DrawUpDownBtnModule ()
		{
			bool clickUp = false;

			bool clickDown = false;

			clickUp = GUILayout.Button (UpGUIContent, GUILayout.Width (BtnSize), GUILayout.Height (BtnSize));

			clickDown = GUILayout.Button (DownGUIContent, GUILayout.Width (BtnSize), GUILayout.Height (BtnSize));

			if (clickUp)
			{
				return UpDownModuleStatus.Up;
			}

			if (clickDown)
			{
				return UpDownModuleStatus.Down;
			}

			return UpDownModuleStatus.None;
		}

		public enum UpDownModuleStatus
		{
			Up,Down,None
		}

		/// <summary>
		/// 確認是否可以被往後位移
		/// </summary>
		/// <returns><c>true</c>, if index to next condition was moved, <c>false</c> otherwise.</returns>
		/// <param name="totalCount">Total count.</param>
		/// <param name="index">Index.</param>
		public static bool MoveIndexToNextCondition (int index, int totalCount)
		{
			//要往後移一位 所以要多準備保留一個index
			if (index <= totalCount - 2) 
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// 確認是否可以被往前位移
		/// </summary>
		/// <returns><c>true</c>, if index to previous condition was moved, <c>false</c> otherwise.</returns>
		/// <param name="totalCount">Total count.</param>
		/// <param name="index">Index.</param>
		public static bool MoveIndexToPrevCondition (int index, int totalCount)
		{
			//要往前移一位 所以不能是0
			if (index <= totalCount - 1 && index != 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static void SetSerializablePoint(SerializedProperty rootProperty,string fieldName,SerializablePoint point)
		{
			SerializedProperty pointProperty = rootProperty.FindPropertyRelative (fieldName);

			SetSerializableVector3 (pointProperty, "position",point.position);
			SetSerializableVector3 (pointProperty, "rotation", point.rotation);
		}

		public static void SetSerializableVector3(SerializedProperty rootProperty,string fieldName,SerializableVector3 v3)
		{
			SerializedProperty v3Property = rootProperty.FindPropertyRelative (fieldName);
			v3Property.FindPropertyRelative ("x").floatValue = v3.x;
			v3Property.FindPropertyRelative ("y").floatValue = v3.y;
			v3Property.FindPropertyRelative ("z").floatValue = v3.z;
		}

		public static SerializablePoint GetSerializablePoint(SerializedProperty rootProperty,string fieldName)
		{
			SerializablePoint point = new SerializablePoint ();

			SerializedProperty pointProperty = rootProperty.FindPropertyRelative (fieldName);

			point.position = GetSerializableVector3 (pointProperty, "position");
			point.rotation = GetSerializableVector3 (pointProperty, "rotation");

			return point;
		}

		static SerializableVector3 GetSerializableVector3(SerializedProperty rootProperty,string fieldName)
		{
			SerializableVector3 v3 = new SerializableVector3();

			SerializedProperty v3Property = rootProperty.FindPropertyRelative (fieldName);

			v3.x = v3Property.FindPropertyRelative ("x").floatValue;
			v3.y = v3Property.FindPropertyRelative ("y").floatValue;
			v3.z = v3Property.FindPropertyRelative ("z").floatValue;

			return v3;
		}

		public static void InvokeInNoIndentLevel(Action innerAction)
		{
			int originIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			innerAction.Invoke ();
			EditorGUI.indentLevel = originIndentLevel;
		}

		#region Dialog Message

		/// <summary>
		/// 此關卡含有隨機群組 請問是否套用隨機
		/// </summary>
		public static Func<bool> RandomDialogCondition = () => 
		{
			return EditorUtility.DisplayDialog ("Title", "此關卡含有隨機群組 請問是否套用隨機", 
				"yes", "no");
		};

		/// <summary>
		/// 與其有相關的關聯都會被撤銷, 確定要刪除設定檔嗎?
		/// </summary>
		public static Func<bool> RemoveTransferDialogCondition = () => 
		{
			return EditorUtility.DisplayDialog ("Title", "與其有相關的關聯都會被撤銷, 確定要刪除設定檔嗎?",
				"yes", "no");
		};

		/// <summary>
		/// 移除後不能還原 確定要移除嗎?
		/// </summary>
		public static Func<bool> RemoveDialogCondition = () => 
		{
			return EditorUtility.DisplayDialog ("Title", "移除後不能還原 確定要移除嗎?",
				"yes", "no");
		};

		/// <summary>
		/// 場景中尚未存儲的變更會遺失 確定繼續嗎?
		/// </summary>
		public static Func<bool> RefreshDialogCondition = () => 
		{
			return EditorUtility.DisplayDialog ("Title", "場景中尚未存儲的變更會遺失 確定繼續嗎?",
				"yes", "no");
		};

		/// <summary>
		/// 是否同步修改舊有資料
		/// </summary>
		public static Func<bool> SyncOldDataDialogCondition = () => 
		{
			return EditorUtility.DisplayDialog ("Title", "是否同步修改舊有資料?",
				"yes", "no");
		};

		/// <summary>
		/// 禁止綁定自身參考
		/// </summary>
		public static void SelfRefBlockMessage ()
		{
			EditorUtility.DisplayDialog ("Title", "禁止綁定自身參考", "ok");
		}

		/// <summary>
		/// 禁止綁定已經綁定過的連接
		/// </summary>
		public static void RepeatRefBlockMessage ()
		{
			EditorUtility.DisplayDialog ("Title", "禁止綁定已經綁定過的連接", "ok");
		}

		/// <summary>
		/// 禁止使用已被使用的名稱
		/// </summary>
		public static void SameNameBlockMessage ()
		{
			EditorUtility.DisplayDialog ("Title", "禁止使用已被使用的名稱", "ok");
		}

		/// <summary>
		/// 禁止使用已被使用的名稱
		/// </summary>
		public static bool SameNameBlock (List<string> existKeys, string newKey)
		{
			if (existKeys.Contains (newKey)) 
			{
				SameNameBlockMessage ();
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// 禁止填入空值
		/// </summary>
		public static void TempValueBlockMessage ()
		{
			EditorUtility.DisplayDialog ("Title", "禁止填入空值", "ok");
		}

		/// <summary>
		/// 物件連結遺失
		/// </summary>
		public static void NullRefBlockMessage ()
		{
			EditorUtility.DisplayDialog ("Title", "物件連結遺失", "ok");
		}

		/// <summary>
		/// 此物件不符合
		/// </summary>
		public static void NotableBlockMessage ()
		{
			EditorUtility.DisplayDialog ("Title", "此物件不符合", "ok");
		}

		#endregion

		public static Texture2D MakeTexture (int width, int height, Color color)
		{
			Color[] pixels = new Color[width * height];

			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] = color;
			}

			Texture2D texture = new Texture2D (width, height);
			texture.SetPixels (pixels);
			texture.Apply ();

			return texture;
		}
	}

    [Serializable]
    public class SlotTogglesCache
    {
        [SerializeField]
        List<SlotToggle> cache = new List<SlotToggle> ();

        //List是ref type 要用value type的比較法
        bool CheckIsSameKey (List<int> key1, List<int> key2)
        {
            if (key1.Count != key2.Count)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < key2.Count; i++)
                {
                    if (key1[i] != key2[i])
                        return false;
                }

                return true;
            }
        }

        public void Set (int key, bool value)
        {
            Set (new List<int> { key }, value);
        }

        public void Set (int[] key, bool value)
        {
            Set (new List<int> (key), value);
        }

        public void Set (List<int> key, bool value)
        {
            var finder = cache.Find (pair => CheckIsSameKey (pair.Key, key));

            if (finder == null)
            {
                finder = new SlotToggle (key);
                finder.Key = key;
                cache.Add (finder);
            }

            finder.Value = value;
        }

        public bool Get (int key)
        {
            return Get (new List<int> { key });
        }

        public bool Get (int[] key)
        {
            return Get (new List<int> (key));
        }

        public bool Get (List<int> key)
        {
            var finder = cache.Find (pair => CheckIsSameKey (pair.Key, key));

            if (finder == null)
            {
                return false;
            }
            else
            {
                return finder.Value;
            }
        }

        public void Remove (int key)
        {
            Remove (new List<int> { key });
        }

        public void Remove (List<int> compareKey)
        {
            //依據root刪除緩存 root符合就刪除 後面的不管ex 傳入0 -> 01 00 都要刪除
            cache.RemoveAll (pair =>
            {
                //我是 12 傳入11121212 所以留著
                if (pair.Key.Count < compareKey.Count)
                {
                    return false;
                }

                for (int i = 0; i < compareKey.Count; i++)
                {
                    //不是最後一位
                    if (i != compareKey.Count - 1)
                    {
                        //我是 123 傳入111 所以留著
                        if (pair.Key[i] != compareKey[i])
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //我是 1112,111 傳入111 所以刪掉
                        if (pair.Key[i] == compareKey[i])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                //不應該進到這裡
                Debug.LogError (JsonUtility.ToJson (pair));
                return false;
            });

            ProcessAfterRemove (compareKey);
        }

        void ProcessAfterRemove (List<int> compareKey)
        {
            //如果最後那位以外的位數不一樣的話 直接跳過 找出前幾位一樣最後那位比目標還大的
            List<SlotToggle> slotToggles = cache.FindAll (pair =>
            {
                //我是 12 傳入11121212 所以不動
                if (pair.Key.Count < compareKey.Count)
                {
                    return false;
                }

                for (int i = 0; i < compareKey.Count; i++)
                {
                    //不是最後一位
                    if (i != compareKey.Count - 1)
                    {
                        //我是 123 傳入111 所以不動
                        if (pair.Key[i] != compareKey[i])
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //我是 113 傳入111 所以要取出來處理
                        if (pair.Key[i] > compareKey[i])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                //不應該進到這裡
                Debug.LogError (JsonUtility.ToJson (pair));
                return false;
            });

            //剩下的以key的最後一位作依據 把對應的位數往前移一格
            slotToggles.ForEach (pair =>
            {
                pair.Key[compareKey.Count - 1] -= 1;
            });
        }

        public void Insert (int key, bool value = false)
        {
            Insert (new List<int> { key }, value);
        }

        public void Insert (List<int> key, bool value = false)
        {
            ProcessBeforeInsert (key);
            cache.Add (new SlotToggle (key, value));
        }

        void ProcessBeforeInsert (List<int> compareKey)
        {
            //如果最後那位以外的位數不一樣的話 直接跳過 找出前幾位一樣最後那位比目標還大的
            List<SlotToggle> slotToggles = cache.FindAll (pair =>
            {
                //我是 12 傳入11121212 所以不動
                if (pair.Key.Count < compareKey.Count)
                {
                    return false;
                }

                for (int i = 0; i < compareKey.Count; i++)
                {
                    //不是最後一位
                    if (i != compareKey.Count - 1)
                    {
                        //我是 123 傳入111 所以不動
                        if (pair.Key[i] != compareKey[i])
                        {
                            return false;
                        }
                    }
                    else
                    {
                        //我是 113 傳入111 所以要取出來處理
                        //跟Remove不同的是 111要先變成112 所以相等要處理
                        if (pair.Key[i] >= compareKey[i])
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                //不應該進到這裡
                Debug.LogError (JsonUtility.ToJson (pair));
                return false;
            });

            //剩下的以key的最後一位作依據 把對應的位數往前移一格
            slotToggles.ForEach (pair =>
            {
                pair.Key[compareKey.Count - 1] += 1;
            });
        }
    }

    [Serializable]
    public class SlotToggle
    {
        public SlotToggle (IEnumerable<int> key, bool value = false)
        {
            this.Key = new List<int> (key);
            this.Value = value;
        }

        public List<int> Key;
        public bool Value;
    }
#endif
}