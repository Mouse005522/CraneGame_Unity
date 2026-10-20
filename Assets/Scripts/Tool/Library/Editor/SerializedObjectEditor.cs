using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Kun.Tool
{
    public abstract class SerializedObjectEditor<T> : Editor where T:UnityEngine.Object 
	{
		protected T runtimeScript;

		protected GUIStyle buttonGUIStyle
		{
			get
			{
				return EditorStyles.miniButton;
			}
		}

		protected GUILayoutOption buttonWidth
		{
			get
			{
				return GUILayout.Width (220);
			}
		}

		protected GUILayoutOption buttonHeight
		{
			get
			{
				return GUILayout.Height (50);
			}
		}

		protected GUIStyle fieldNameGUIStyle
		{
			get
			{
				return EditorStyles.label;
			}
		}

		protected GUIStyle titleNameGUIStyle
		{
			get
			{
				return EditorStyles.label;
			}
		}

		protected GUIStyle boxSkin
		{
			get
			{
				return GUI.skin.box;
			}
		}

		GUIContent switchGUIContent;

		/// <summary>
		/// 齒輪狀 icon
		/// </summary>
		/// <value>The content of the switch GUI.</value>
		protected GUIContent SwitchGUIContent
		{
			get 
			{
				if (switchGUIContent == null)
				{
					switchGUIContent = EditorGUIUtility.IconContent ("d__Popup");
				}

				return switchGUIContent;
			}
		}

		protected const float ArrowBtnWidth = 24f;
		protected const float ArrowBtnHeight = 24f;

		public GUIContent UpGUIContent
		{
			get
			{
				if (upGUIContent == null)
				{
					upGUIContent = EditorGUIUtility.IconContent ("ProfilerTimelineRollUpArrow");
				}

				return upGUIContent;
			}
		}

		GUIContent upGUIContent;

		public GUIContent DownGUIContent
		{
			get
			{
				if (downGUIContent == null)
				{
					downGUIContent = EditorGUIUtility.IconContent ("ProfilerTimelineDigDownArrow");
				}

				return downGUIContent;
			}
		}

		GUIContent downGUIContent;

		protected const float fieldNameWidth = 70f;

		protected virtual void OnEnable()
		{
			runtimeScript = target as T;
		}

		protected void DrawVariableField (string variableName, Action drawAndGetInput, float? overrideFieldWidth = null)
		{
			EditorTool.DrawInHorizontal (() => 
			{
				EditorGUILayout.LabelField (variableName, fieldNameGUIStyle, GUILayout.Width (fieldNameWidth));
				drawAndGetInput.Invoke ();
			});
		}

		protected void SetDirty (bool setSceneToo)
		{
			EditorUtility.SetDirty (serializedObject.targetObject);

			if (setSceneToo) 
			{
				Scene activeScene = EditorSceneManager.GetActiveScene ();
				EditorSceneManager.MarkSceneDirty (activeScene);
			}
		}

		protected void SaveScene ()
		{
            EditorUtility.SetDirty (runtimeScript);

            Scene activeScene = EditorSceneManager.GetActiveScene ();
			EditorSceneManager.MarkSceneDirty (activeScene);
			EditorSceneManager.SaveScene (activeScene);
			AssetDatabase.SaveAssets ();
			EditorSceneManager.SaveOpenScenes ();
		}

		/// <summary>
		/// 確認是否可以被往後位移
		/// </summary>
		/// <returns><c>true</c>, if index to next condition was moved, <c>false</c> otherwise.</returns>
		/// <param name="totalCount">Total count.</param>
		/// <param name="index">Index.</param>
		protected bool MoveIndexToNextCondition (int index, int totalCount)
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
		protected bool MoveIndexToPrevCondition (int index, int totalCount)
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
	}	
}