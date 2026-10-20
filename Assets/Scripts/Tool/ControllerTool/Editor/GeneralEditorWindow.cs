using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kun.Tool
{
    public class GeneralEditorWindow : EditorWindow
	{
		public static float fieldNameWidth = 150f;
		public static float fieldKeyWidth = 80f;
		public static float buttonWidth = 100f;
		public static float defaultButtonHigh = 50f;
		public static float flatButtonHigh = 20f;

		public static float scriptFieldKeyWidth = 50f;

		public static Vector2 scrollPos = new Vector2 ();
		public static float scrollBarHeight = 500f;

		public static float settingContentHeight = 800f;

		public static GUIStyle FieldNameGUIStyle
		{
			get
			{
				return EditorStyles.label;
			}
		}


		static GUIStyle richFieldNameGUIStyle;
		public static GUIStyle RichFieldNameGUIStyle
		{
			get
			{
				if (richFieldNameGUIStyle == null)
				{
					richFieldNameGUIStyle = new GUIStyle (EditorStyles.label);
					richFieldNameGUIStyle.richText = true;
				}

				return richFieldNameGUIStyle;
			}
		}

		static GUIStyle orFieldNameGUIStyle = null;

		public static GUIStyle OrFieldNameGUIStyle
		{
			get
			{
				if (orFieldNameGUIStyle == null)
				{
					orFieldNameGUIStyle = new GUIStyle (EditorStyles.label);
					orFieldNameGUIStyle.normal.textColor = Color.blue;
				}
				return orFieldNameGUIStyle;
			}
		}

		static GUIStyle andFieldNameGUIStyle = null;

		public static GUIStyle AndFieldNameGUIStyle
		{
			get
			{
				if (andFieldNameGUIStyle == null)
				{
					andFieldNameGUIStyle = new GUIStyle (EditorStyles.label);
					andFieldNameGUIStyle.normal.textColor = Color.red;
				}
				return andFieldNameGUIStyle;
			}
		}

		GUIStyle worryFieldNameGUIStyle;

		protected GUIStyle WorryFieldNameGUIStyle
		{
			get
			{
				if (worryFieldNameGUIStyle == null)
				{
					worryFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);

					worryFieldNameGUIStyle.normal.textColor = Color.red;
					worryFieldNameGUIStyle.fontStyle = FontStyle.Bold;
				}

				return worryFieldNameGUIStyle;
			}
		}

		GUIStyle worryBigFieldNameGUIStyle;

		protected GUIStyle WorryBigFieldNameGUIStyle
		{
			get
			{
				if (worryBigFieldNameGUIStyle == null)
				{
					worryBigFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);

					worryBigFieldNameGUIStyle.normal.textColor = Color.red;
					worryBigFieldNameGUIStyle.fontStyle = FontStyle.Bold;
					worryBigFieldNameGUIStyle.fontSize = 15;
				}

				return worryBigFieldNameGUIStyle;
			}
		}

		GUIStyle tapBigFieldNameGUIStyle;

		protected GUIStyle TapBigFieldNameGUIStyle
		{
			get
			{
				if (tapBigFieldNameGUIStyle == null)
				{
					tapBigFieldNameGUIStyle = new GUIStyle (TitleNameGUIStyle);

					tapBigFieldNameGUIStyle.normal.textColor = new Color (0.3137f, 0.6212f, 0.8867f);
					tapBigFieldNameGUIStyle.fontStyle = FontStyle.Bold;
					tapBigFieldNameGUIStyle.fontSize = 15;
				}

				return tapBigFieldNameGUIStyle;
			}
		}


		public static GUIStyle TitleNameGUIStyle
		{
			get
			{
				return EditorStyles.boldLabel;
			}
		}

		protected GUIStyle DefaultButtonGUIStyle
		{
			get
			{
				return EditorStyles.miniButton;
			}
		}

		public static GUIStyle FlatButtonGUIStyle
		{
			get
			{
				return EditorStyles.toolbarButton;
			}
		}

		protected GUIStyle RadioButtonGUIStyle
		{
			get
			{
				return EditorStyles.radioButton;
			}
		}


		public static GUIStyle BoxGUIStyle
		{
			get
			{
				return GUI.skin.box;
			}
		}

		const float SearchFieldWidth = 200f;
		const float SearchFieldHeight = 10f;

        Dictionary<object, bool> folderToggle = new Dictionary<object, bool> ();

        /// <summary>
        /// 用data本身當key決定顯示的開關
        /// 傳入的data都要是class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFolder"></param>
        protected void SetFolderToggle (object key, bool isFolder)
        {
            folderToggle[key] = isFolder;
        }

        /// <summary>
        /// 用data本身當key決定顯示的開關
        /// 傳入的data都要是class
        /// </summary>
        protected bool GetFolderToggle (object key, bool defaultValue = false)
        {
            if (folderToggle.ContainsKey (key) == false)
            {
                folderToggle.Add (key, defaultValue);
            }

            return folderToggle[key];
        }

		Dictionary<object, Vector3> scrollViewCache = new Dictionary<object, Vector3> ();

        /// <summary>
        /// 用data本身當key決定顯示的開關
        /// 傳入的data都要是class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isFolder"></param>
        protected void SetScrollViewCache (object key, Vector3 pos)
        {
            scrollViewCache[key] = pos;
        }

        /// <summary>
        /// 用data本身當key決定顯示的開關
        /// 傳入的data都要是class
        /// </summary>
        protected Vector3 GetScrollViewCache (object key)
        {
            if (scrollViewCache.ContainsKey (key) == false)
            {
                scrollViewCache.Add (key, Vector3.zero);
            }

            return scrollViewCache[key];
        }

        public static string SearchField (string value, float? width = null, float? height = null)
		{
			float _width = 0f;

			if (width == null)
			{
				_width = SearchFieldWidth;
			}
			else
			{
				_width = width.Value;
			}

			float _height = 0f;

			if (height == null)
			{
				_height = SearchFieldHeight;
			}
			else
			{
				_height = height.Value;
			}

			string newValue = GUILayout.TextField (value, (GUIStyle)"ToolbarSeachTextField", GUILayout.Width (_width), GUILayout.Height (_height));
			return newValue;
		}

		static GUIStyle orBoxGUIStyle;
		public static GUIStyle OrBoxGUIStyle
		{
			get
			{
				if (orBoxGUIStyle == null)
				{
					orBoxGUIStyle = new GUIStyle (BoxGUIStyle);
					orBoxGUIStyle.normal.background = MakeTex (600, 1, new Color (0.8392f, 0.9249f, 1, 1));
				}

				return orBoxGUIStyle;
			}
		}

		static GUIStyle andBoxGUIStyle;
		public static GUIStyle AndBoxGUIStyle
		{
			get
			{
				if (andBoxGUIStyle == null)
				{
					andBoxGUIStyle = new GUIStyle (BoxGUIStyle);
					andBoxGUIStyle.normal.background = MakeTex (600, 1, Color.red);
				}

				return andBoxGUIStyle;
			}
		}

		static Texture2D MakeTex (int width, int height, Color col)
		{
			Color [] pix = new Color [width * height];

			for (int i = 0; i < pix.Length; i++)
				pix [i] = col;

			Texture2D result = new Texture2D (width, height);
			result.SetPixels (pix);
			result.Apply ();

			return result;
		}

		protected float areaInterval = 10f;

		protected float buttonHorzInterval = 30f;

		int originIndentLevel;

		protected void SetIndentLevelZero ()
		{
			originIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
		}

		protected void RevertIndentLevel ()
		{
			EditorGUI.indentLevel = originIndentLevel;
		}

		protected void DrawSerializablePoint (SerializedProperty rootProperty, string fieldName)
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

		protected void DrawSerializableVector3 (SerializedProperty rootProperty, string fieldName)
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

		protected void DrawSerializableQuaternion (SerializedProperty rootProperty, string fieldName)
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

		protected void DrawVariableField (string variableName, Action drawAndGetInput, float? overrideFieldWidth = null)
		{
			EditorGUILayout.BeginHorizontal ();
			{
				EditorGUILayout.LabelField (variableName, FieldNameGUIStyle, GUILayout.Width (fieldNameWidth));
				drawAndGetInput.Invoke ();
			}
			EditorGUILayout.EndHorizontal ();
		}

		protected void DrawCustomSerlizedField (string key, SerializedProperty rootProperty)
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

		protected class RemoveCache
		{
			public int? hasRemoveIndex;

			event Action<int> removeAtEvent;

			public RemoveCache (Action<int> removeAtEvent)
			{
				this.removeAtEvent = removeAtEvent;
			}

			public void Flush ()
			{
				if (hasRemoveIndex != null)
				{
					if (removeAtEvent != null)
						removeAtEvent.Invoke (hasRemoveIndex.Value);

					hasRemoveIndex = null;
				}
			}
		}

		protected class CollectionModifyCache
		{
			public bool hasAdd = false;
			public int? removeIndex = null;
			public int? cloneIndex = null;
		}

		public static void InvokeInNoIndentLevel (Action innerAction)
		{
			int originIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			innerAction.Invoke ();
			EditorGUI.indentLevel = originIndentLevel;
		}

		public class GroupScrollView
		{
			float scrollBarHigh;

			public GroupScrollView (string scrollViewFieldName, SerializedObject bindSerializedObject, float scrollBarHigh)
			{
				this.scrollViewFieldName = scrollViewFieldName;
				this.bindSerializedObject = bindSerializedObject;

				this.scrollBarHigh = scrollBarHeight;
				this.Init ();
			}

			public List<GameObject> GetCurrentSelectObjects ()
			{
				List<GameObject> selects = new List<GameObject> ();

				for (int i = 0; i < currentGroup.editorPairObjects.Count; i++)
				{
					if (toggle [i])
					{
						selects.Add ((GameObject)currentGroup.editorPairObjects [i].value);
					}
				}
				return selects;
			}

			#region set by cotr
			string scrollViewFieldName;
			SerializedObject bindSerializedObject;
			#endregion

			#region property
			SerializedProperty scrollViewProperty
			{
				get
				{
					return bindSerializedObject.FindProperty (scrollViewFieldName);
				}
			}
			#endregion

			List<EditorGroup> editorGroups = new List<EditorGroup> ();

			EditorGroup currentGroup;

			const float scrollBarHeight = 800f;
			public const float scrollBarWidth = 300f;

			int currentGroupIndex;

			void SwitchCurrentGroup (int index)
			{
				currentGroup.Dispose ();

				currentGroup = editorGroups [index];
				currentGroup.Refresh ();
				currentGroupIndex = index;

				RefreshSelectAllStatus ();
			}

			bool [] toggle;

			void DrawSinglePrefab (SerializedProperty item, int index)
			{
				UnityEngine.Object prefab = item.objectReferenceValue;
				string prefabName = prefab.name;

				EditorGUILayout.BeginVertical (BoxGUIStyle);
				{
					EditorGUILayout.BeginHorizontal ();
					{
						EditorGUILayout.LabelField ($"{prefabName}", TitleNameGUIStyle);

						bool originToggle = toggle [index];

						bool newToggle = EditorGUILayout.Toggle ("", originToggle, GUILayout.Width (15));

						if (newToggle != originToggle)
						{
							toggle [index] = newToggle;
						}
					}
					EditorGUILayout.EndHorizontal ();

					Editor editor = currentGroup.editorPairObjects [index].key;
					editor.OnPreviewGUI (GUILayoutUtility.GetRect (150, 150f), (GUIStyle)"WindowBackground");
				}
				EditorGUILayout.EndVertical ();
			}

			bool open = true;

			public void Init ()
			{
				// name
				// repository
				editorGroups = new List<EditorGroup> ();

				scrollViewProperty.Map ((item, index) =>
				{
					EditorGroup newEditorGroup = new EditorGroup (bindSerializedObject, scrollViewFieldName, index);
					editorGroups.Add (newEditorGroup);
				});

				currentGroupIndex = 0;
				currentGroup = editorGroups [0];
				RefreshSelectAllStatus ();
			}

			void RefreshSelectAllStatus ()
			{
				toggle = new bool [currentGroup.editorPairObjects.Count];
				isSelectTurn = false;
			}

			bool isSelectTurn = false;

			void SwitchSelectAllStatus ()
			{
				isSelectTurn = !isSelectTurn;

				for (int i = 0; i < toggle.Length; i++)
				{
					toggle [i] = isSelectTurn;
				}
			}

			public void Draw ()
			{
				EditorGUILayout.BeginVertical ();
				{
					EditorGUILayout.BeginHorizontal (BoxGUIStyle, GUILayout.Width (scrollBarWidth));
					{
						List<string> options = new List<string> ();

						List<GUIContent> guiContents = new List<GUIContent> ();

						editorGroups.ForEach (editorGroup => options.Add (editorGroup.groupName));

						int newInedex = 0;

						InvokeInNoIndentLevel (() =>
						{
							newInedex = EditorGUILayout.Popup (currentGroupIndex, options.ToArray (), GUILayout.Width (100f));
						});

						if (newInedex != currentGroupIndex)
						{
							SwitchCurrentGroup (newInedex);
						}

						GUILayout.FlexibleSpace ();

						if (GUILayout.Button ("Select", FlatButtonGUIStyle, GUILayout.Height (flatButtonHigh), GUILayout.Width (buttonWidth)))
						{
							SwitchSelectAllStatus ();
						}

						GUILayout.Space (15);
					}
					EditorGUILayout.EndHorizontal ();

					scrollPos = EditorGUILayout.BeginScrollView (scrollPos, new GUIStyle (), GUILayout.Height (scrollBarHigh), GUILayout.Width (scrollBarWidth));
					{
						currentGroup.arrayProperty.Map (DrawSinglePrefab);
					}
					EditorGUILayout.EndScrollView ();
				}
				EditorGUILayout.EndVertical ();
			}

			public void Dispose ()
			{
				editorGroups.ForEach (editorGroup => editorGroup.Dispose ());
			}
		}

		#region Dialog Message

		protected Func<bool> RandomDialogCondition = () =>
		{
			return EditorUtility.DisplayDialog ("Title", "此關卡含有隨機群組 請問是否套用隨機",
				"yes", "no");
		};

		protected Func<bool> RemoveDialogCondition = () =>
		{
			return EditorUtility.DisplayDialog ("Title", "移除後不能還原 確定要移除嗎?",
				"yes", "no");
		};

		#endregion

		protected class RemoveAndCloneCache
		{
			public RemoveAndCloneCache (Action<int> removeCallback, Action<int> cloneCallback)
			{
				this.removeCallback = removeCallback;
				this.cloneCallback = cloneCallback;

				removeIndex = null;
				cloneIndex = null;
			}

			public int? RemoveIndex
			{
				set
				{
					removeIndex = value;
				}
			}

			public int? CloneIndex
			{
				set
				{
					cloneIndex = value;
				}
			}

			int? removeIndex;
			int? cloneIndex;

			Action<int> removeCallback;
			Action<int> cloneCallback;

			public void Flush ()
			{
				if (removeIndex != null)
				{
					removeCallback.Invoke (removeIndex.Value);
				}

				if (cloneIndex != null)
				{
					cloneCallback.Invoke (cloneIndex.Value);
				}
			}
		}
	}

	public class EditorGroup
	{
		public string groupName;

		public List<RefKeyValuePair<Editor, UnityEngine.Object>> editorPairObjects = new List<RefKeyValuePair<Editor, UnityEngine.Object>> ();

		public List<SerializedProperty> itemPropertys = new List<SerializedProperty> ();

		SerializedObject serializedObject;

		SerializedProperty collectionProperty
		{
			get
			{
				return serializedObject.FindProperty (rootName).GetArrayElementAtIndex (index);
			}
		}

		public SerializedProperty arrayProperty
		{
			get
			{
				return collectionProperty.FindPropertyRelative ("repository");
			}
		}

		string rootName;
		int index;

		public EditorGroup (SerializedObject serializedObject, string rootName, int index)
		{
			this.serializedObject = serializedObject;
			this.rootName = rootName;
			this.index = index;

			this.groupName = collectionProperty.FindPropertyRelative ("name").stringValue;
			editorPairObjects = new List<RefKeyValuePair<Editor, UnityEngine.Object>> ();

			this.arrayProperty.Foreach ((item) =>
			{
				UnityEngine.Object prefab = item.objectReferenceValue;
				Editor previewEditor = Editor.CreateEditor (prefab);
				editorPairObjects.Add (new RefKeyValuePair<Editor, UnityEngine.Object> (previewEditor, prefab));
			});
		}

		public void Dispose ()
		{
			editorPairObjects.ForEach (pair =>
			{
				MonoBehaviour.DestroyImmediate (pair.key);
			});

			editorPairObjects.Clear ();
		}

		public void Refresh ()
		{
			Dispose ();
			arrayProperty.Foreach ((item) =>
			{
				UnityEngine.Object prefab = item.objectReferenceValue;
				Editor previewEditor = Editor.CreateEditor (prefab);
				editorPairObjects.Add (new RefKeyValuePair<Editor, UnityEngine.Object> (previewEditor, prefab));
			});
		}
    }
}