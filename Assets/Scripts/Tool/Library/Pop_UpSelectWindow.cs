using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kun.Tool
{
#if UNITY_EDITOR
    public class Pop_UpSelectWindow : EditorWindow
	{
		public static void ShowWindow (Action<string> flushCallback, string currentValue, IEnumerable<string> originItems, IEnumerable<string> comments = null)
		{
			Pop_UpSelectWindow customSelectWindow = GetWindow<Pop_UpSelectWindow> ("Pop_UpSelectWindow");

			Action<int> processIndexCallback = (int index) =>
			{
				if (flushCallback != null)
				{
					string value = "";

					if (index != -1)
					{
						value = originItems.ToList ()[index];
						flushCallback.Invoke (value);
					}
				}
			};

			int currentIndex = originItems.ToList ().IndexOf (currentValue);

			customSelectWindow.Init (processIndexCallback, currentIndex, originItems.ToList (), comments != null ? comments.ToList () : null);
			customSelectWindow.Show ();
		}

		/// <summary>
		/// 如果傳入的選項有重覆的可能 建議透過index回傳值 才不會混淆
		/// </summary>
		/// <param name="flushCallback">Flush callback.</param>
		/// <param name="currentIndex">Current index.</param>
		/// <param name="originItems">Origin items.</param>
		/// <param name="comments">Comments.</param>
		public static void ShowWindow (Action<int> flushCallback, int currentIndex, IEnumerable<string> originItems, IEnumerable<string> comments = null)
		{
			Pop_UpSelectWindow customSelectWindow = GetWindow<Pop_UpSelectWindow> ("Pop_UpSelectWindow");
			customSelectWindow.Init (flushCallback, currentIndex, originItems.ToList (), comments != null ? comments.ToList () : null);
			customSelectWindow.Show ();
		}


		string cacheSearch;

		NodeTree originTree;
		NodeTree curTree;

		List<NodeValue> curNodeValues = new List<NodeValue> ();

		Action<int> flushIndexCallback;

		int currentIndex;

        void OnEnable ()
        {
			InitGUIStyle ();
		}

        void Init (Action<int> flushCallback, int currentIndex, List<string> originItems, List<string> comments)
		{
			this.flushIndexCallback = flushCallback;
			originTree = new NodeTree ();
			originTree.InputMsgs (originItems, comments);

			this.currentIndex = currentIndex;

			scrollPos = new Vector2 (0, 0);

			PrepareCurTree ("");
		}

		Vector2 scrollPos = Vector2.zero;

		void PrepareCurTree (string searchKey) 
		{
			curTree = originTree.DeepClone ();

			if (string.IsNullOrEmpty (searchKey) == false) 
			{
				curTree.ProcessSearchKey (searchKey.ToLower ());
			}

			curNodeValues = curTree.GetSubValues ();

			if (curNodeValues.Exists (curNodeValue => curNodeValue.index == currentIndex) == false)
			{
				currentIndex = -1;
			}
		}

		void OnGUI ()
		{
			Event curEvent = Event.current;

			if (curEvent.keyCode == KeyCode.Escape) 
			{
				setClose = true;
			}

			if (setClose)
			{
				ClearEvent (curEvent);
				return;
			}

			ProcessEvent (curEvent);

			DrawSearchField ();

			scrollPos = EditorTool.DrawInScrollView (scrollPos, 750f, () =>
			{
				DrawNode (curTree, curEvent, 0);
			});

			//點到視窗外了
			if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow != this)
			{
				setClose = true;
			}

			ClearEvent (curEvent);
		}

		void ProcessEvent (Event e) 
		{
			if (e.type == EventType.KeyDown)
			{
				if (curNodeValues.Count > 0) 
				{
					var oldIndexof = curNodeValues.FindIndex (nodeValue => nodeValue.index == currentIndex);

					if (e.keyCode == KeyCode.UpArrow)
					{
						if (oldIndexof > 0)
						{
							currentIndex = curNodeValues[oldIndexof - 1].index;
						}
					}

					if (e.keyCode == KeyCode.DownArrow)
					{
						if (oldIndexof < curNodeValues.Count - 1)
						{
							currentIndex = curNodeValues[oldIndexof + 1].index;
						}
					}
				}

				if (e.keyCode == KeyCode.Return)
				{
					Flush ();
				}
			}
		}

		void Update ()
		{
			if (setClose)
			{
				Close ();
			}
		}

		void DrawSearchField ()
		{
			EditorTool.DrawInHorizontal (() =>
			{
				EditorGUILayout.LabelField ("Search : ", GUILayout.Width (70), GUILayout.Height (GeneralEditor.flatButtonHigh));
				var newSearch = EditorGUILayout.TextField (cacheSearch, EditorStyles.textField, GUILayout.Height (GeneralEditor.flatButtonHigh));

				if (newSearch != cacheSearch)
				{
					cacheSearch = newSearch;

					PrepareCurTree (cacheSearch);
				}
			});
		}

		bool setClose;

		void DrawNode (NodeTree nodeTree, Event curEvent, int indent)
		{
			float space = 5 * indent;

			string titleMsg = string.IsNullOrEmpty (nodeTree.nodeName) == false ? nodeTree.nodeName : "無群組";

			bool isRoot = nodeTree.prevTree == null;

			bool newFolder = false;

			if (isRoot == false)
			{
				EditorTool.DrawInHorizontal (() =>
				{
					EditorGUILayout.Space (space);

					GUIContent titllIcon = nodeTree.isShow ? titleShowFolderIcon : titleHideFolderIcon;

					GUIStyle style = new GUIStyle (EditorStyles.foldout);

					EditorTool.DrawInVertical (()=> 
					{
						GUILayout.Space (3f);
						// folder不能制定寬度,
						// toggle+folder會偏上
						newFolder = GUILayout.Toggle (nodeTree.isShow, "", style, GUILayout.Width (10f));
					});

					EditorGUILayout.LabelField (titleMsg);

					GUILayout.FlexibleSpace ();
				});
			}

			if (nodeTree.isShow || isRoot)
			{
				nodeTree.nextNodes.ForEach (nextNode =>
				{
					DrawNode (nextNode, curEvent, indent + 1);
				});

				nodeTree.values.ForEach (nodeValue =>
				{
					EditorTool.DrawInHorizontal (() =>
					{
						if (isRoot == false)
						{
							EditorGUILayout.Space (space * 2 + 15f);
						}
						DrawItem (nodeValue, curEvent);
						GUILayout.FlexibleSpace ();
					});
				});
			}

			if (newFolder!= nodeTree.isShow) 
			{
				nodeTree.isShow = newFolder;
			}
		}

		void DrawItem (NodeValue nodeValue, Event curEvent)
		{
			GUIStyle guiStyle;

			if (nodeValue.index == currentIndex)
			{
				guiStyle = selectedStyle;
			}
			else
			{
				guiStyle = normalStyle;
			}

			string msg = nodeValue.GetShowMsg ();

			EditorGUILayout.LabelField (msg, guiStyle, GUILayout.Height (GeneralEditor.flatButtonHigh));

			Rect rect = GUILayoutUtility.GetLastRect ();
			if (curEvent.type == EventType.MouseDown && curEvent.button == 0)
			{
				if (rect.Contains (curEvent.mousePosition))
				{
					currentIndex = nodeValue.index;
					curEvent.Use ();

					if (curEvent.clickCount >= 2)
					{
						Flush ();
					}
				}
			}
		}

		void Flush ()
		{
			flushIndexCallback.Invoke (currentIndex);

			setClose = true;
		}

		void ClearEvent (Event currentEvent)
		{
			if (currentEvent.type != EventType.Repaint && currentEvent.type != EventType.Layout) 
			{
				currentEvent.Use ();
			}
		}

		GUIStyle normalStyle;

		GUIStyle selectedStyle;

		GUIStyle titleStyle;

		GUIContent titleHideFolderIcon;
		GUIContent titleShowFolderIcon;

		//靜態建構式, 用來共用的GUIStyle
		void InitGUIStyle ()
		{
			var selectIcon = new Texture2D (1, 1);
			Color origanColor = new Color (1, 0.517701f, 0, 1f);
			selectIcon.SetPixels (new Color[] { origanColor });
			selectIcon.Apply ();

			normalStyle = new GUIStyle (EditorStyles.label);

			selectedStyle = new GUIStyle (normalStyle);

			selectedStyle.normal.background = selectIcon;

			selectedStyle.normal.textColor = Color.black;

			titleStyle = new GUIStyle (normalStyle);
			//titleStyle.normal.textColor = Color.white;

			var titleIcon = new Texture2D (1, 1);
			Color titleColor = Color.green;
			titleColor.a = 0.3f;
			titleIcon.SetPixels (new Color[] { titleColor });
			titleIcon.Apply ();

			titleStyle.normal.background = titleIcon;

			titleHideFolderIcon = new GUIContent (EditorGUIUtility.FindTexture ("d_PlayButton"));
			titleShowFolderIcon = new GUIContent (EditorGUIUtility.FindTexture ("d_dropdown"));
		}
	}
#endif

	[Serializable]
	class NodeTree
	{
		public string nodeName = "";

		public bool isShow = true;

		public NodeTree prevTree = null;

		public List<NodeTree> nextNodes = new List<NodeTree> ();

		public List<NodeValue> values = new List<NodeValue> ();

		public void InputMsgs (List<string> msgs, List<string> commits)
		{
			msgs.ToList ().Map ((msgIndex, msg) =>
			{
				var splits = msg.Split ("/").ToList ();

				NodeTree curNode = this;

				splits.Map ((index, split, isLast) =>
				{
					if (isLast)
					{
						string commit = "";

						if (commits != null && msgIndex <= commits.Count - 1)
						{
							commit = commits[msgIndex];
						}

						//把最底層的物件獨立到一個群組叫作無群組
						if (curNode.prevTree == null)
						{
							if (curNode.nextNodes.TryFind (node => node.nodeName == "", out NodeTree findNode) == false)
							{
								findNode = new NodeTree () { nodeName = "", prevTree = curNode };
								curNode.nextNodes.Add (findNode);
							}

							findNode.values.Add (new NodeValue () { msg = split, commit = commit, index = msgIndex });
						}
						else
						{
							curNode.values.Add (new NodeValue () { msg = split, commit = commit, index = msgIndex });
						}
					}
					else
					{
						if (curNode.nextNodes.TryFind (node => node.nodeName == split, out NodeTree findNode) == false)
						{
							findNode = new NodeTree () { nodeName = split, prevTree = curNode };
							curNode.nextNodes.Add (findNode);
						}

						curNode = findNode;
					}
				});
			});

			var curNode = this;
		}

		public List<NodeValue> GetSubValues () 
		{
			List<NodeValue> subValues = new List<NodeValue> ();

			nextNodes.ForEach (nextNode => subValues.AddRange (nextNode.GetSubValues ()));
			subValues.AddRange (values);

			return subValues;
		}

		public void ProcessSearchKey (string searchKey)
		{
			//如果資料夾名稱有包含key那整組都會被算
			if (nodeName.ToLower ().Contains (searchKey) == false)
			{
				values.RemoveAll (value => value.msg.ToLower ().Contains (searchKey) == false);

				nextNodes.ForEach (nextNode => nextNode.ProcessSearchKey (searchKey));

				//因為搜尋導致底下沒有東西了
				nextNodes.RemoveAll (nextNode => nextNode.nextNodes.Count == 0 && nextNode.values.Count == 0);
			}
		}
	}

	[Serializable]
	class NodeValue 
	{
		public string msg;
		public int index;
		public string commit;

		/// <summary>
		/// 包含註解的顯示訊息
		/// </summary>
		/// <returns></returns>
		public string GetShowMsg () 
		{
			if (string.IsNullOrEmpty (commit)) 
			{
				return msg;
			}
			else
			{
				return $"{msg}	-------->	{commit}";
			}
		}
	}
}