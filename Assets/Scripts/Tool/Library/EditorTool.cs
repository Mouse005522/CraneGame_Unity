using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace Kun.Tool
{
    public static class EditorTool 
	{
		#if UNITY_EDITOR
		public static float HorizontalIndentSpace = 14f;

		static float fieldNameWidth = 150f;
		static float fieldKeyWidth = 80f;
		static GUIStyle fieldNameGUIStyle
		{
			get
			{
				return EditorStyles.miniLabel;
			}
		}
		static GUIStyle titleNameGUIStyle
		{
			get
			{
				return EditorStyles.boldLabel;
			}
		}
		static BindingFlags checkEnumBindFlags = BindingFlags.Public | BindingFlags.Static;

		public static T GetCacheData<T> () where T:ScriptableObject
		{
			T data = null;
			string typeName = typeof(T).Name;
			string[] guids = AssetDatabase.FindAssets ($"t:{typeName}");

			if (guids == null || guids.Length == 0) 
			{
				Debug.LogError ($"can't get data, type -> {typeName}");
				return null;
			} 
			else if (guids.Length > 1)
			{
				Debug.LogError ($"has multi same data, type -> {typeName}");
				return null;
			}
			else
			{
				string dataPath = AssetDatabase.GUIDToAssetPath (guids [0]);
				data = AssetDatabase.LoadAssetAtPath<T> (dataPath);
			}

			return data;
		}

        public static Vector3 DrawVector3 (Vector3 value, string fieldName)
        {
			EditorGUILayout.LabelField (fieldName);

            EditorTool.DrawInHorizontal (() =>
            {
				EditorGUILayout.LabelField ("X", titleNameGUIStyle, GUILayout.Width (12f));
				value.x = EditorGUILayout.FloatField (value.x, GUILayout.Width (100f));

				GUILayout.Space (10f);

                EditorGUILayout.LabelField ("Y", titleNameGUIStyle, GUILayout.Width (12f));
                value.y = EditorGUILayout.FloatField (value.y, GUILayout.Width (100f));

                GUILayout.Space (10f);

                EditorGUILayout.LabelField ("Z", titleNameGUIStyle, GUILayout.Width (12f));
                value.z = EditorGUILayout.FloatField (value.z, GUILayout.Width (100f));
            });

            return value;
        }

        /// <summary>
        /// 畫在非layout上
        /// 如果有掛載EnumMsg則會顯示掛載的字串,
        /// 沒掛載就用enum直接ToString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        public static T DrawEnum<T> (T oldValue, Rect pos) where T : Enum
        {
            var msgs = EnumMsgExtension.GetMsgs<T> ();
            var intValues = EnumMsgExtension.GetMsgIntValues<T> ();

            var oldIntValue = Convert.ToInt32 (oldValue);
            var oldIndex = System.Array.IndexOf (intValues, oldIntValue);

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            var index = EditorGUI.Popup (pos, oldIndex, msgs);

            return (T)Enum.ToObject (typeof (T), intValues[index]);
        }

        /// <summary>
        /// 如果有掛載EnumMsg則會顯示掛載的字串,
        /// 沒掛載就用enum直接ToString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        public static T DrawEnum<T> (T oldValue, string title , params GUILayoutOption[] layout) where T : Enum
        {
            var msgs = EnumMsgExtension.GetMsgs<T> ();
            var intValues = EnumMsgExtension.GetMsgIntValues<T> ();

            var oldIntValue = Convert.ToInt32 (oldValue);
            var oldIndex = System.Array.IndexOf (intValues, oldIntValue);

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            int index = layout == null ? EditorGUILayout.Popup (oldIndex, msgs) : EditorGUILayout.Popup (title, oldIndex, msgs, layout);

            return (T)Enum.ToObject (typeof (T), intValues[index]);
        }

		public static void DrawSearchableDropdown(List<string> options, string oldOption, Action<string> onFinish, Action<string> onDrawOption )
        {

            var oldIndex = options.IndexOf(oldOption);

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            int newIndex = oldIndex;

            DrawInHorizontal(() =>
            {
				if (onDrawOption != null)
				{
					onDrawOption?.Invoke(options[oldIndex]);
				}
				else
				{
					if (options.Count > oldIndex)
					{

						GUILayout.Label(options[oldIndex]);
					}
					else
					{
                        GUILayout.Label(oldOption);
					}
				}

                if (GUILayout.Button(EditorGUIUtility.IconContent("d__Popup"), GUILayout.Width(30)))
                {
                    Pop_UpSelectWindow.ShowWindow((int index) =>
                    {
                        newIndex = index;
                        var newValue = options[newIndex];

                        onFinish.Invoke(newValue);

                    }, newIndex, options);
                }
            });
        }

        public static void DrawSearchableDropdown(List<string> options, string oldOption, Action<string> onFinish, string title, float defaultNameWidth = 150, float defaultKeyWidth = 80)
		{

            DrawSearchableDropdown(options, oldOption, onFinish, (string option) =>
			{
                EditorGUILayout.LabelField(title, GUILayout.Width(defaultNameWidth));
                EditorGUILayout.LabelField(option, GUILayout.Width(defaultKeyWidth));
            });
        }

        /// <summary>
        /// 把Type的常數做成下拉式選項畫出來
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldValue"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void DrawConstFieldRect<TType, TValue> (Rect position,TValue oldValue, Action<TValue> onFinish, string title = "",
            float defaultNameWidth = 150, float defaultKeyWidth = 80, params TValue[] culls)
        {
            var keys = ConstFieldTable<TType, TValue>.keys;

            var values = ConstFieldTable<TType, TValue>.values;

            if (culls.Length > 0)
            {
                keys = keys.ToList ();

                values = values.ToList ();

                foreach (var cull in culls)
                {
                    var index = values.IndexOf (cull);
                    values.RemoveAt (index);
                    keys.RemoveAt (index);
                }
            }

            var oldIndex = values.IndexOf (oldValue);

            if (oldIndex < 0)
            {
                oldIndex = 0;
            }

            int newIndex = oldIndex;

			position.width = defaultKeyWidth;
			EditorGUI.LabelField (position, title);

			position.center += Vector2.right * 100;
			position.width = defaultKeyWidth;

            EditorGUI.LabelField (position, keys[oldIndex]);

			position.center += Vector2.right * 100;
			position.width = 30;

			var iconContent = EditorGUIUtility.IconContent ("d__Popup");

			if (GUI.Button (position, iconContent))
            {
                Pop_UpSelectWindow.ShowWindow ((int index) =>
                {
                    newIndex = index;
                    var newValue = values[newIndex];

                    onFinish.Invoke (newValue);

                }, newIndex, keys);
            }
        }

        /// <summary>
        /// 把Type的常數做成下拉式選項畫出來
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldValue"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void DrawConstField<TType, TValue> (TValue oldValue, Action<TValue> onFinish, string title = "",
            float defaultNameWidth = 150, float defaultKeyWidth = 80, params TValue[] culls)
        {
			var keys = ConstFieldTable<TType, TValue>.keys;

			var values = ConstFieldTable<TType, TValue>.values;

			if (culls.Length > 0) 
			{
				keys = keys.ToList ();

				values = values.ToList ();

				foreach (var cull in culls) 
				{
					var index = values.IndexOf (cull);
					values.RemoveAt (index);
					keys.RemoveAt (index);
				}
			}

			var oldIndex = values.IndexOf (oldValue);

			if (oldIndex < 0) 
			{
                oldIndex = 0;
            }

			int newIndex = oldIndex;

			DrawInHorizontal (() => 
			{
				EditorGUILayout.LabelField (title, GUILayout.Width (defaultNameWidth));
                EditorGUILayout.LabelField (keys[oldIndex], GUILayout.Width (defaultKeyWidth));

				if (GUILayout.Button (EditorGUIUtility.IconContent ("d__Popup"), GUILayout.Width (30)))
				{
					Pop_UpSelectWindow.ShowWindow ((int index) =>
					{
						newIndex = index;
						var newValue = values[newIndex];

                        onFinish.Invoke (newValue);

                    }, newIndex, keys);
                }
            });
        }

		public static void DrawGeneralObject (object source,string fieldName)
		{
			GUILayout.Label (fieldName, titleNameGUIStyle, GUILayout.Width (fieldNameWidth));

			EditorGUI.indentLevel++;

			Type type = source.GetType ();

			if (!type.IsSerializable)
				throw new Exception("not system Serializable");

			List<FieldInfo> fieldInfos = new List<FieldInfo> (type.GetFields());

			fieldInfos.ForEach ((fieldInfo)=>
				{
					Type fieldType = fieldInfo.FieldType;

					if(fieldType.IsArray)
					{
						DrawArrayItem(fieldInfo,source);
					}
					else
					{
						if(fieldType.IsGenericType)
						{
							if(fieldType.IsList())
							{
								DrawListItem(fieldInfo,source);
							}
							else
							{
								Debug.LogError("尚未支援array以及list以外的容器");
							}
						}
						else
						{
							DrawSingleItem(fieldInfo,source);
						}
					}
				});

			EditorGUI.indentLevel--;
		}

		static void DrawListItem(FieldInfo fieldInfo,object source)
		{
			Type fieldType = fieldInfo.FieldType;

			object listObject = fieldInfo.GetValue(source);

			int count = Convert.ToInt32(fieldType.GetProperty("Count").GetValue(listObject));

			PropertyInfo propertyInfo = fieldType.GetProperty ("Item");

			for (int i = 0; i < count; i++) 
			{
				object item = propertyInfo.GetValue (listObject, new object[]{ i });

				DrawSingleRepeatedItme (item,(obj)=>
					{
						propertyInfo.SetValue(listObject,obj, new object[]{ i });
					});
			}
		}

		static void DrawArrayItem(FieldInfo fieldInfo,object source)
		{
			Type fieldType = fieldInfo.FieldType;

			Array arrayObject = (Array)fieldInfo.GetValue (source);

			for (int i = 0; i < arrayObject.Length; i++) 
			{
				object item = arrayObject.GetValue (i);

				DrawSingleRepeatedItme (item,(obj)=>
					{
						arrayObject.SetValue(obj,i);
					});
			}
		}


		static void DrawSingleRepeatedItme(object item,Action<object> onValueModify)
		{
			Type fieldType = item.GetType ();

			if (fieldType.IsArray || (fieldType.IsGenericType)) 
			{
				Debug.LogError ("尚未支援多重容器");
			} 
			else
			{
				if (fieldType.IsClass)
				{
					if (fieldType == typeof(String))
					{
						string oldValue = (string)item;

						//TODO 字串
						string newValue= EditorGUILayout.TextField (oldValue, GUILayout.Width (fieldKeyWidth));

						if(newValue!=oldValue)
						{
							onValueModify.Invoke (newValue);
						}
					}
					else
					{
						//TODO Custom Class
					}
				}
				else if(fieldType.IsPrimitive)
				{
					//TODO 字串以外的基礎型別
					if(fieldType==typeof(int))
					{
						int oldValue = (int)item;

						int newValue = EditorGUILayout.IntField ("", oldValue, GUILayout.Width (fieldKeyWidth));

						if(newValue!=oldValue)
						{
							onValueModify.Invoke (newValue);
						}
					}
					else if (fieldType == typeof(float))
					{
						float oldValue = (float)item;

						float newValue = EditorGUILayout.FloatField ("",oldValue, GUILayout.Width (fieldKeyWidth));

						if(newValue!=oldValue)
						{
							onValueModify.Invoke (newValue);
						}
					}
				}
				else
				{
					if(fieldType.IsEnum)
					{
						int oldValue = (int)item;

						FieldInfo[] fieldInfos = fieldType.GetFields (checkEnumBindFlags);

						string[] enumFieldNames = new string[fieldInfos.Length];

						for (int i = 0; i < fieldInfos.Length; i++) 
						{
							enumFieldNames [i] = fieldInfos [i].Name;
						}

						int newValue = EditorGUILayout.Popup (oldValue, enumFieldNames, GUILayout.Width (fieldKeyWidth));

						if(newValue!=oldValue)
						{
							onValueModify.Invoke (newValue);
						}
					}
					else
					{
						Debug.LogError ("尚未支援Sturct");
					}
				}
			}
		}

		static void DrawSingleItem (FieldInfo fieldInfo,object source)
		{
			string fieldName = fieldInfo.Name;
			Type fieldType = fieldInfo.FieldType;
			object fieldValue= fieldInfo.GetValue(source);

			if (fieldType.IsClass)
			{
				if (fieldType == typeof(String))
				{
					string oldValue = (string)fieldValue;

					//TODO 字串
					string newValue= DrawStringField(fieldName,oldValue);

					if(newValue!=oldValue)
					{
						fieldInfo.SetValue(source,newValue);
					}
				}
				else
				{
					//TODO Custom Class
				}
			}
			else if(fieldType.IsPrimitive)
			{
				//TODO 字串以外的基礎型別
				if(fieldType==typeof(int))
				{
					int oldValue = (int)fieldValue;

					int newValue = DrawIntField(fieldName,oldValue);

					if(newValue!=oldValue)
					{
						fieldInfo.SetValue(source,newValue);
					}
				}
				else if (fieldType == typeof(float))
				{
					float oldValue = (float)fieldValue;

					float newValue = DrawFloatField(fieldName,oldValue);

					if(newValue!=oldValue)
					{
						fieldInfo.SetValue(source,newValue);
					}
				}
			}
			else
			{
				if(fieldType.IsEnum)
				{
					int oldValue = (int)fieldValue;

					int newValue = DrawEnumField(fieldType,fieldName,oldValue);

					if(newValue!=oldValue)
					{
						fieldInfo.SetValue(source,newValue);
					}
				}
				else
				{
					Debug.LogError ("尚未支援Sturct");
				}
			}
		}

		static bool IsList(this Type type)
		{
			return type.GetGenericTypeDefinition () == typeof(List<>);
		}


		public static string DrawStringField (string fieldName, string fieldValue)
		{
			string result = "";

			Action drawInputPart = () => 
			{
				result = EditorGUILayout.TextField (fieldValue, GUILayout.Width (fieldKeyWidth));
			};

			DrawField (fieldName, drawInputPart);

			return result;
		}

		public static int DrawIntField (string fieldName, int fieldValue)
		{
			int result = 0;

			Action drawInputPart = () => 
			{
				result = EditorGUILayout.IntField ("",fieldValue, GUILayout.Width (fieldKeyWidth));
			};

			DrawField (fieldName, drawInputPart);

			return result;
		}

		public static float DrawFloatField (string fieldName, float fieldValue)
		{
			float result = 0f;

			Action drawInputPart = () => 
			{
				result = EditorGUILayout.FloatField ("",fieldValue, GUILayout.Width (fieldKeyWidth));
			};

			DrawField (fieldName, drawInputPart);

			return result;
		}

		public static string[] GetEnumFields(Type enumType)
		{
			FieldInfo[] fieldInfos = enumType.GetFields (checkEnumBindFlags);

			string[] enumFieldNames = new string[fieldInfos.Length];

			for (int i = 0; i < fieldInfos.Length; i++) 
			{
				enumFieldNames [i] = fieldInfos [i].Name;
			}

			return enumFieldNames;
		}

		public static int DrawEnumField (Type enumType,string fieldName, int selectValue)
		{
			string[] enumFieldNames = GetEnumFields (enumType);

			int result = 0;

			Action drawInputPart = () => 
			{
				result = EditorGUILayout.Popup(selectValue,enumFieldNames,GUILayout.Width(fieldKeyWidth));
			};

			DrawField (fieldName, drawInputPart);

			return result;
		}

		static void DrawField(string fieldName,Action callback)
		{
			string processField = fieldName;

			fieldNameGUIStyle.alignment = TextAnchor.MiddleLeft;

			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label (processField, fieldNameGUIStyle, GUILayout.Width (fieldNameWidth));
				callback.Invoke ();
			}
			GUILayout.EndHorizontal ();
		}

		public static void DrawInDisable (Action callback)
		{
			GUI.enabled = false;
			callback.Invoke ();
			GUI.enabled = true;
		}

		public static AnimatorController GetAnimatorController (this Animator anim)
		{
			var runtimeController = anim.runtimeAnimatorController;

			if (runtimeController == null) 
			{
				Debug.LogError ("runtimeController is null");
				return null;
			}
			
			string path = AssetDatabase.GetAssetPath (runtimeController);

			AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController> (path);

			return controller;
		}

		public class GeneralGUILayout
		{
			public static GUIStyle button = EditorStyles.miniButton;
			public static GUIStyle titleName = EditorStyles.label;
			public static GUIStyle fieldName = EditorStyles.miniLabel;
			public static float  classSpace = 10f;
		}

		public static void Foreach (this SerializedProperty arrayProperty,Action<SerializedProperty> loopAction)
		{
			int originSize = arrayProperty.arraySize;

			for (int i = 0; i < originSize; i++) 
			{
				SerializedProperty item = arrayProperty.GetArrayElementAtIndex (i);
				loopAction.Invoke (item);
			}
		}

		public static void Map (this SerializedProperty arrayProperty,Action<SerializedProperty,int> loopAction)
		{
			int originSize = arrayProperty.arraySize;

			for (int i = 0; i < originSize; i++) 
			{
				SerializedProperty item = arrayProperty.GetArrayElementAtIndex (i);
				loopAction.Invoke (item, i);
			}
		}


		public static SerializedProperty AddNewOne(this SerializedProperty arrayProperty)
		{
			int originSize = arrayProperty.arraySize;

			arrayProperty.InsertArrayElementAtIndex (originSize);

			return arrayProperty.GetArrayElementAtIndex (originSize);
		}

		public static SerializedProperty Find (this SerializedProperty arrayProperty, string queryFieldName, int value)
		{
			SerializedProperty findProperty = null;

			findProperty = FindInternal (arrayProperty,(item)=>
				{
					return item.FindPropertyRelative(queryFieldName).intValue == value;
				});

			return findProperty;
		}

		public static SerializedProperty Find (this SerializedProperty arrayProperty, string queryFieldName, float value)
		{
			SerializedProperty findProperty = null;

			findProperty = FindInternal (arrayProperty,(item)=>
				{
					return item.FindPropertyRelative(queryFieldName).floatValue == value;
				});

			return findProperty;
		}

		public static SerializedProperty Find (this SerializedProperty arrayProperty, string queryFieldName, string value)
		{
			SerializedProperty findProperty = null;

			findProperty = FindInternal (arrayProperty,(item)=>
				{
					return item.FindPropertyRelative(queryFieldName).stringValue == value;
				});

			return findProperty;
		}

		static SerializedProperty FindInternal(SerializedProperty arrayProperty,Func<SerializedProperty,bool> condition)
		{
			int originSize = arrayProperty.arraySize;

			SerializedProperty findProperty = null;

			for (int i = 0; i < originSize; i++) 
			{
				SerializedProperty item = arrayProperty.GetArrayElementAtIndex (i);

				if (condition.Invoke (item)) 
				{
					findProperty = item;
					break;
				}
			}

			return findProperty;
		}

		/// <summary>
		/// 取得陣列底下值
		/// </summary>
		/// <returns>The values.</returns>
		/// <param name="arrayProperty">Array property.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> GetValues<T> (this SerializedProperty arrayProperty)
		{
			List<T> values = new List<T> ();

			for (int i = 0; i < arrayProperty.arraySize; i++) 
			{
				object value = null;
				
				var itemProperty = arrayProperty.GetArrayElementAtIndex (i);

				var type = typeof(T);

				if (type == typeof(float)) 
				{
					value = itemProperty.floatValue;
				}
				else if (type == typeof(int)) 
				{
					value = itemProperty.intValue;
				}
				else if (type == typeof(bool)) 
				{
					value = itemProperty.boolValue;
				}
				else if (type == typeof(string)) 
				{
					value = itemProperty.stringValue;
				}
				else if (type == typeof(Vector2)) 
				{
					value = itemProperty.vector2Value;
				}
				else if (type == typeof(Vector3)) 
				{
					value = itemProperty.vector3Value;
				}
				else if (type.IsSubclassOf(typeof(UnityEngine.Object))) 
				{
					value = itemProperty.objectReferenceValue;
				}
				else
				{
					Debug.LogError ("not support type -> " + type.ToString ());
					return values;
				}

				T tValue = (T)value;

				values.Add (tValue);
			}

			return values;
		}

		/// <summary>
		/// 設定list值
		/// </summary>
		/// <param name="arrayProperty">Array property.</param>
		/// <param name="values">Values.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void SetValues<T> (this SerializedProperty arrayProperty, List<T> values)
		{
			arrayProperty.ClearArray ();

			for (int i = 0; i < values.Count; i++) 
			{
				var value = values [i];	

				object obj = value;

				var type = typeof(T);

				if (type == typeof(float)) 
				{
					arrayProperty.AddNewOne ().floatValue = (float)obj;
				}
				else if (type == typeof(int)) 
				{
					arrayProperty.AddNewOne ().intValue = (int)obj;
				}
				else if (type == typeof(bool)) 
				{
					arrayProperty.AddNewOne ().boolValue = (bool)obj;
				}
				else if (type == typeof(string)) 
				{
					arrayProperty.AddNewOne ().stringValue = (string)obj;
				}
				else if (type == typeof(Vector2)) 
				{
					arrayProperty.AddNewOne ().vector2Value = (Vector2)obj;
				}
				else if (type == typeof(Vector3)) 
				{
					arrayProperty.AddNewOne ().vector3Value = (Vector3)obj;
				}
				else if (type.IsSubclassOf(typeof(UnityEngine.Object))) 
				{
					arrayProperty.AddNewOne ().objectReferenceValue = (UnityEngine.Object)obj;
				}
				else
				{
					Debug.LogError ("not support type -> " + type.ToString ());
					return;
				}

				values.Add (value);
			}
		}

		public static void DrawInHorizontal (Action body, GUIStyle style = null, params GUILayoutOption[] options)
		{

			if (style == null)
			{
				EditorGUILayout.BeginHorizontal (options);
			}
			else
			{
				EditorGUILayout.BeginHorizontal (style, options);
			}

			body.Invoke ();
			EditorGUILayout.EndHorizontal ();
		}

		public static void DrawInReadOnly(Action body)
		{
			GUI.enabled = false;
			body.Invoke ();
			GUI.enabled = true;
		}

		public static void DrawInVertical(Action body, GUIStyle style = null, params GUILayoutOption[] options)
		{
			if (style == null)
			{
				EditorGUILayout.BeginVertical (options);
			}
			else
			{
				EditorGUILayout.BeginVertical (style, options);
			}

			body.Invoke();
			EditorGUILayout.EndVertical();
		}

		public static void DrawInProperty (Rect position, SerializedProperty property, GUIContent label, Action body)
		{
			EditorGUI.BeginProperty (position, label, property);
			{
				body.Invoke ();
			}
			EditorGUI.EndProperty ();
		}

		public static void DrawInNoIndent (Action body)
		{
			int originIndent = EditorGUI.indentLevel;

			EditorGUI.indentLevel = 0;
			{
				body.Invoke ();
			}
			EditorGUI.indentLevel = originIndent;
		}

		public static void DrawInIndent (Action body)
		{
			EditorGUI.indentLevel++;
			{
				body.Invoke ();
			}
			EditorGUI.indentLevel--;
		}

		public static Vector2 DrawInScrollView (Vector2 scorllPosition, float scrollBarHeight , float scrollBarWith, Action body)
		{
			Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope(scorllPosition, new GUIStyle(), GUILayout.Height(scrollBarHeight), GUILayout.Width(scrollBarWith)))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke();
            }

			return newPosition;
		}

		public static Vector2 DrawInScrollView (Vector2 scorllPosition, float scrollBarHeight, Action body)
		{
			Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope(scorllPosition, GUILayout.Height(scrollBarHeight)))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke();
            }

			return newPosition;
		}

        public static Vector2 DrawInScrollView(Vector2 scorllPosition, Action body, params GUILayoutOption[] options)
        {
            Vector2 newPosition;

            using (var scrollView = new GUILayout.ScrollViewScope(scorllPosition, options))
            {
                newPosition = scrollView.scrollPosition;

                body.Invoke();
            }

            return newPosition;
        }

		public static void DrawInScrollView(string scrollKey, Action body)
		{
            Vector3 pos = GetScrollViewCache(scrollKey);

            pos = DrawInScrollView(pos, body);

            SetScrollViewCache(scrollKey, pos);
        }

		private static Dictionary<object, Vector3> scrollViewCache = new Dictionary<object, Vector3>();

        /// <summary>
        /// 用data本身當key決定顯示的開關
        /// 傳入的data都要是class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFolder"></param>
        private static void SetScrollViewCache(object key, Vector3 pos)
        {
            scrollViewCache[key] = pos;
        }

        /// <summary>
        /// 用data本身當key決定顯示的開關
        /// 傳入的data都要是class
        /// </summary>
        private static Vector3 GetScrollViewCache(object key)
        {
            if (scrollViewCache.ContainsKey(key) == false)
            {
                scrollViewCache.Add(key, Vector3.zero);
            }

            return scrollViewCache[key];
        }

        public static void DrawInHandles (Action body)
		{
			Handles.BeginGUI ();
			{
				body.Invoke ();
			}
			Handles.EndGUI ();
		}

		public static void DrawInColor (Color drawColor, Action body)
		{
			Color originColor = GUI.color;
			GUI.color = drawColor;

			body.Invoke ();

			GUI.color = originColor;
		}

		public static void DrawInHandlesColor (Color drawColor, Action body)
		{
			Color originColor = Handles.color;
			Handles.color = drawColor;

			body.Invoke ();

			Handles.color = originColor;
		}

		public static void DrawInGizmosColor (Color drawColor, Action body)
		{
			Color originColor = Gizmos.color;
			Gizmos.color = drawColor;

			body.Invoke ();

			Gizmos.color = originColor;
		}

		public static void DrawInMatrix (Matrix4x4 matrix, Action body)
		{
			Matrix4x4 originMatrix = Handles.matrix;

			Handles.matrix = matrix;

			body.Invoke ();

			Handles.matrix = originMatrix;
		}

		public static void DrawInGizmosMatrix (Matrix4x4 matrix, Action body)
		{
			Matrix4x4 originMatrix = Gizmos.matrix;

			Gizmos.matrix = matrix;

			body.Invoke ();

			Gizmos.matrix = originMatrix;
		}

		public static void OpenScriptAsset<T> ()
		{
			OpenScriptAsset (typeof(T));
		}

		public static void OpenScriptAsset (Type type) 
		{
			foreach (string assetPath in AssetDatabase.GetAllAssetPaths())
			{
				if (CheckTypeByPath (type, assetPath))
				{
					var script = (MonoScript)AssetDatabase.LoadAssetAtPath(assetPath, typeof(MonoScript));
					if (script != null)
					{
						AssetDatabase.OpenAsset (script);
						break;
					}
				}
			}
		}

		static bool CheckTypeByPath (System.Type type , string path)
		{
			string[] splitPaths = path.Split (new char[]{ '/' });

			string endPointFileName = splitPaths [splitPaths.Length - 1];

			string processFileName = endPointFileName.Replace (".cs", "");

			return (processFileName == type.Name);
		}

		/// <summary>
		/// EventType 為Valudate
		/// </summary>
		public static bool TryGetCommandType (this Event e, out CommandType commandType) 
		{
			if (e.type == EventType.ValidateCommand)
			{
				string commandName = e.commandName;

				if (string.IsNullOrEmpty (commandName) == false)
				{
					if (Enum.TryParse (commandName, out commandType))
					{
						return true;
					}
					else
					{
						Debug.LogError ("can't parse command -> " + commandName);
					}
				}
			}

			commandType = default;
			return false;
		}

        public static T DrawEnumPopup<T> (SerializedProperty property, string label) where T : System.Enum
        {
            // 取得所有 enum 的訊息
            string[] messages = EnumMsgExtension.GetMsgs<T> ();

            // 取得所有 enum 值
            T[] enumValues = (T[])System.Enum.GetValues (typeof (T));

            // 取得當前選擇的 index
            int currentIndex = property.enumValueIndex;

            // 顯示下拉選單 (使用訊息)
            int selectedIndex = EditorGUILayout.Popup (label, currentIndex, messages);

            // 如果選擇改變,更新 property
            if (selectedIndex != currentIndex)
            {
                property.enumValueIndex = selectedIndex;
            }

			return enumValues[selectedIndex];
        }

        /// <summary>
        /// 繪製只顯示帶有指定 Attribute 的 enum 下拉選單
        /// 利用 EnumFilterHelper 靜態快取，正確對應 SerializedProperty.enumValueIndex
        /// </summary>
        public static T DrawFilteredEnumPopup<T, TAttribute> (SerializedProperty property, string label)
            where T : System.Enum
            where TAttribute : Attribute
        {
            string[] messages = EnumFilterHelper<T, TAttribute>.FilteredMsgs;
            int[] enumIndices = EnumFilterHelper<T, TAttribute>.FilteredEnumIndices;
            var filteredValues = EnumFilterHelper<T, TAttribute>.FilteredValues;

            int currentEnumIndex = property.enumValueIndex;
            int currentFilteredIndex = System.Array.IndexOf (enumIndices, currentEnumIndex);

            if (currentFilteredIndex < 0)
            {
                currentFilteredIndex = 0;
            }

            int selectedFilteredIndex = EditorGUILayout.Popup (label, currentFilteredIndex, messages);

            if (selectedFilteredIndex != currentFilteredIndex)
            {
                property.enumValueIndex = enumIndices[selectedFilteredIndex];
            }

            return filteredValues[selectedFilteredIndex];
        }

#endif
    }

	/// <summary>
	/// e.commandName轉來的, 所以要都宣告一次
	/// </summary>
	public enum CommandType
	{
		Copy,
		Cut,
		Paste,
		SoftDelete,
		FrameSelected,
		Duplicate,
		SelectAll,
		UndoRedoPerformed
	}
}
