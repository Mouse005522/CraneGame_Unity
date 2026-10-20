using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public class ComponentMaskTree
	{
		/// <summary>
		/// 以Component為分界, compoennt底下物件歸他管, 如果底下又有其他compoennt 那就歸底下那個管
		/// </summary>
		/// <returns>The data group.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<ComponentMaskData<T>> CreateDataGroup<T> (List<GameObject> roots) where T : Component
		{
			List<ComponentMaskData<T>> datas = new List<ComponentMaskData<T>> ();

			Action<ComponentMaskData<T>> onCreateData = (data) => datas.Add (data);

			roots.ForEach (root=>
				{
					IterMaskGo<T> (root, null, onCreateData);
				});

			return datas;
		}

		static void IterMaskGo<T> (GameObject go, ComponentMaskData<T> rootData, Action<ComponentMaskData<T>> onCreateData) where T : Component
		{
			var getCom = go.GetComponent<T> ();

			ComponentMaskData<T> curData;

			if (getCom != null) 
			{
				var newData = new ComponentMaskData<T> (getCom, go);

				onCreateData.Invoke (newData);

				curData = newData;
			}
			else
			{
				curData = rootData;

				if (curData != null) 
				{
					//父物件已經產生了Data那就把自己加進去
					curData.gos.Add (go);
				}
			}

			int childCount = go.transform.childCount;

			for (int i = 0; i < childCount; i++) 
			{
				var childGo = go.transform.GetChild (i).gameObject;

				IterMaskGo (childGo, curData, onCreateData);
			}
		}
	}

	public class ComponentMaskData<T> where T : Component
	{
		/// <summary>
		/// Com附屬的那個Component就是列表的第一個物件
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="ownerGo">Owner go.</param>
		public ComponentMaskData (T key, GameObject ownerGo)
		{
			this.key = key;

			gos = new List<GameObject>{ ownerGo };
		}
		
		public T key;

		public List<GameObject> gos = new List<GameObject> ();
	}
}