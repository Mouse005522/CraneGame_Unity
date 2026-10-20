using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    /// <summary>
    ///  把物件轉成 配上一個bool 用以記錄狀態
    /// </summary>
    public class ToggleDataPairTable<T>
	{
		public ToggleDataPairTable (List<T> datas, bool toggle = false)
		{
			this.datas = new List<T> ();

			this.toggles = new List<bool> ();
			
			for (int i = 0; i < datas.Count; i++) 
			{
				this.datas.Add (datas [i]);

				this.toggles.Add (toggle);
			}
		}
		
		public ToggleDataPairTable ()
		{
			
		}
		
		List<T> datas = new List<T> ();

		List<bool> toggles = new List<bool> ();

		public List<T> GetDatas (bool value)
		{
			List<T> getDatas = new List<T> ();

			for (int i = 0; i < toggles.Count; i++) 
			{
				if (toggles [i] == value)
				{
					getDatas.Add (datas[i]);
				}
			}

			return getDatas;
		}

		public List<T> GetDatas ()
		{
			return datas;
		}

		public void Map (Action<int,T,bool> iterCallback)
		{
			for (int i = 0; i < datas.Count; i++) 
			{
				iterCallback.Invoke (i, datas [i], toggles [i]);
			}
		}

		public void Map (Action<int,T> iterCallback)
		{
			for (int i = 0; i < datas.Count; i++) 
			{
				iterCallback.Invoke (i, datas [i]);
			}
		}

		public void ForEach (Action<T,bool> iterCallback)
		{
			for (int i = 0; i < datas.Count; i++) 
			{
				iterCallback.Invoke (datas [i], toggles [i]);
			}
		}

		public void ForEach (Action<T> iterCallback)
		{
			for (int i = 0; i < datas.Count; i++) 
			{
				iterCallback.Invoke (datas [i]);
			}
		}

		public bool Exists (Predicate<T> condition)
		{
			for (int i = 0; i < datas.Count; i++) 
			{
				if (condition.Invoke (datas [i])) 
				{
					return true;
				}
			}

			return false;
		}

		public void Add (T data, bool toggle = false)
		{
			datas.Add (data);
			toggles.Add (toggle);
		}

		public void Remove (T data)
		{
			int index = datas.IndexOf (data);

			if (index != -1) 
			{
				RemoveAt (index);
			}
			else
			{
				Debug.LogError ("remove fail -> {data}");
			}
		}

		public void RemoveAt (int index)
		{
			datas.RemoveAt (index);
			toggles.RemoveAt (index);
		}

		public void SwitchToggle (T data)
		{
			int index = datas.IndexOf (data);

			if (index != -1) 
			{
				SwitchToggleAt (index);
			}
			else
			{
				Debug.LogError ("remove fail -> {data}");
			}
		}

		public void SwitchToggleAt (int index)
		{
			toggles [index] = !toggles [index];
		}

		public bool GetValue (int index)
		{
			return toggles [index];
		}

		/// <summary>
		/// 讓此物件與下一個物件對調
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="moveIndex">Move index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void MoveIndexToNext (int index)
		{
			//要往後移一位 所以要多準備保留一個index
			if (index > datas.Count - 2)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				datas.MoveIndexToNext (index);
				toggles.MoveIndexToNext (index);
			}
		}

		/// <summary>
		/// 讓此物件移到最後一位
		/// </summary>
		/// <param name="index">Index.</param>
		public void MoveIndexToLast (int index)
		{
			//要往後移一位 所以要多準備保留一個index
			if (index > datas.Count - 1)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				datas.MoveIndexToLast (index);
				toggles.MoveIndexToLast (index);
			}
		}

		/// <summary>
		/// 讓此物件與上一個物件對調
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="moveIndex">Move index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void MoveIndexToPrev (int index)
		{
			//0不能往前移
			if (index > datas.Count - 1 || index == 0)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				datas.MoveIndexToPrev (index);
				toggles.MoveIndexToPrev (index);
			}
		}

		/// <summary>
		/// 讓此物件移到第一位
		/// </summary>
		/// <param name="index">Index.</param>
		public void MoveIndexToFirst (int index)
		{
			//0不能往前移
			if (index > datas.Count - 1 || index == 0)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				datas.MoveIndexToFirst (index);
				toggles.MoveIndexToFirst (index);
			}
		}
	}
}
