using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    /// List<T> Editor
    /// </summary>
    public class RepeatDataEditorController<T>
	{	
		public RepeatDataEditorController (List<T> datas, float scrollHeight)
		{
			this.datas = new List<T> (datas);
			this.scrollHeight = scrollHeight;
		}

		public void BindCallback (Action<T, int> drawBody, Func<T> createNew)
		{
			this.drawBody = drawBody;
			this.createNew = createNew;
		}

		public void BindAddCallback (Action<T> onAdd)
		{
			this.onAdd = onAdd;
			this.hasAddCallback = true;
		}

		public void BindRemoveCallback (Action<int> onRemove)
		{
			this.onRemove = onRemove;
			this.hasRemoveCallback = true;
		}

		public void BindCloneCallback (Action<T, T> onCopy, Action<int, T> onInsert = null)
		{
			this.onCopy = onCopy;
			this.onInsert = onInsert != null ? onInsert : (index, item) => { };
			this.hasCloneCallback = true;
		}

		/// <summary>
		/// 當Item本身不是一個ref的時候要透過index才能進來修改
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="newData">New data.</param>
		public void UpdateData (int index, T newData)
		{
			datas [index] = newData;
		}

		public void Draw ()
		{
			Action addNewData = () => 
			{
				var newData = CreateNewData ();

				datas.Add (newData);
				if (hasAddCallback)
				{
					onAdd.Invoke (newData);
				}
			};

			Action<int> removeData = (index) => 
			{
				if (GeneralEditor.RemoveDialogCondition.Invoke ()) 
				{
					datas.RemoveAt (index);
					if (hasRemoveCallback)
					{
						onRemove.Invoke (index);
					}
				}
			};

			Action<int> cloneData = (index) =>
			{
				var sourceData = datas[index];
				var newData = CreateNewData ();
				
				if (hasCloneCallback)
				{
					onCopy.Invoke (sourceData, newData);
				}
				
				datas.Insert (index + 1, newData);
				
				if (hasCloneCallback)
				{
					onInsert.Invoke (index + 1, newData);
				}
			};

			RemoveAddAndCloneCache removeAddAndCloneCache = new RemoveAddAndCloneCache (addNewData, removeData, cloneData);

			using (new GUILayout.VerticalScope(EditorStyles.helpBox))
			{
				if (hasAddCallback)
				{
					EditorTool.DrawInHorizontal(() =>
					{
						if (GUILayout.Button("Add", GUILayout.Width(GeneralEditor.buttonWidth), GUILayout.Height(25)))
						{
							removeAddAndCloneCache.hasAdd = true;
						}

						GUILayout.FlexibleSpace();

					}, "box");
				}


				if (scrollHeight > 0)
				{
                    scrollPos = EditorTool.DrawInScrollView(scrollPos, scrollHeight, () =>
                    {
                        datas.Map((index, cache, isLast) =>
                        {

                            DrawBody(cache, removeAddAndCloneCache, index);

                            if (isLast == false)
                            {
                                GUILayout.Space(5f);
                            }
                        });
                    });
                }
				else
				{
					EditorTool.DrawInScrollView($"{typeof(T)}-ScrollView", () =>
					{
                        datas.Map((index, cache, isLast) =>
                        {

                            DrawBody(cache, removeAddAndCloneCache, index);

                            if (isLast == false)
                            {
                                GUILayout.Space(5f);
                            }
                        });
                    });
				}

				
			}

			removeAddAndCloneCache.Flush ();
		}

		protected virtual void DrawBody (T item, RemoveAddAndCloneCache removeAddAndCloneCache, int index)
		{
			GUILayout.Space (10);

			EditorTool.DrawInHorizontal (() =>
				{
					EditorTool.DrawInVertical (() =>
						{
							drawBody.Invoke (item, index);
						});

					if (hasCloneCallback || hasRemoveCallback)
					{
						EditorTool.DrawInVertical (() =>
						{
							if (hasCloneCallback)
							{
								if (GUILayout.Button ("Clone", GUILayout.Width (GeneralEditor.buttonWidth), GUILayout.Height (25)))
								{
									removeAddAndCloneCache.hasCloneIndex = index;
								}
							}

							if (hasRemoveCallback)
							{
								if (GUILayout.Button ("Remove", GUILayout.Width (GeneralEditor.buttonWidth), GUILayout.Height (25)))
								{
									removeAddAndCloneCache.hasRemoveIndex = index;
								}
							}
						});
					}

				}, GeneralEditor.BoxGUIStyle);
		}

		protected virtual T CreateNewData ()
		{
			var newData = createNew.Invoke ();

			return newData;
		}

		#region 設定資料
		protected string titleName;

		public List<T> Datas => datas;

		protected List<T> datas = new List<T> ();
		
		protected float scrollHeight;
		#endregion

		#region callback
		protected Action<T,int> drawBody;
		protected Func<T> createNew;
		protected Action<T> onAdd;
		protected Action<int> onRemove;
		protected Action<T, T> onCopy;
		protected Action<int, T> onInsert;
		
		protected bool hasAddCallback;
		protected bool hasRemoveCallback;
		protected bool hasCloneCallback;
		#endregion

		#region 緩存變數
		protected Vector2 scrollPos;
		#endregion
	}
}
