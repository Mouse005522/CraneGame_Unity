using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Kun.Tool
{
    /// <summary>
    /// 有字串當唯一key的list的Drawer
    /// </summary>
    public class TableEditorController<T> : RepeatDataEditorController<T> where T : IKeyable
	{	
		public TableEditorController (List<T> datas, float scrollHeight) : base (datas, scrollHeight)
		{
		}

        public TableEditorController(List<T> datas) : base(datas, 0)
        {
        }

        protected override void DrawBody (T item, RemoveAddAndCloneCache removeAddAndCloneCache, int index)
		{
			//先畫Key, 再來畫物件本體
			GUILayout.Space (10);

			EditorTool.DrawInHorizontal (() =>
            {
                var newKey = EditorGUILayout.TextField (item.ItemKey, GUILayout.Width (150));

                if (newKey != item.ItemKey)
                {
                    var existKeys = GetExistKeys ();

                    if (existKeys.Contains (newKey) == false)
                    {
                        item.ItemKey = newKey;
                    }
                    else
                    {
                        GeneralEditor.SameNameBlockMessage ();
                    }
                }

				base.DrawBody (item, removeAddAndCloneCache, index);
            });
		}

		List<string> GetExistKeys()
		{
			return datas.ConvertAll (data => data.ItemKey);
		}

		protected override T CreateNewData ()
		{
			var existNames = datas.ConvertAll (data => data.ItemKey);

			var newName = GuidFactory.CreateGUID (existNames);

			var newData = createNew.Invoke ();
			newData.ItemKey = newName;

			return newData;
		}
	}
}
