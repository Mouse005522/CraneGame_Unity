using System.Collections.Generic;

namespace Kun.Tool
{
    public class RandomTool
	{
		public static List<int> GetRange (int count)
		{
			List<int> range = new List<int> ();

			for (int i = 0; i < count; i++) 
			{
				range.Add (i);
			}

			return range;
		}
	}

	/// <summary>
	/// 利用底層維護一個Suffle池, 
	/// 每次都可以得到一個新的物件, 全部物件都輪一次才會再來,
	/// 且保證這次值的開頭不要跟上次的尾相同的話要記住這個值
	/// </summary>
	public class ShufflePool<T>
	{
		public ShufflePool (List<T> datas)
		{
			suffleGroup = new SuffleGroup<T> (datas);

			curIndex = -1;
			curDatas = suffleGroup.GetSuffle ();
		}

		public T GetNewData ()
		{
			curIndex++;

			if (curIndex >= curDatas.Count) 
			{
				curDatas = suffleGroup.GetSuffle ();
				curIndex = 0;
			}

			return curDatas [curIndex];
		}

		int curIndex;
		List<T> curDatas = new List<T> ();

		SuffleGroup<T> suffleGroup;
	}

	public class SuffleGroup<T>
	{
		public SuffleGroup (List<T> datas)
		{
			this.datas = new List<T> (datas);	
			this.lastEndIndex = -1;
		}

		List<T> datas;

		/// <summary>
		/// 希望這次值的開頭不要跟下次的尾相同的話要記住這個值
		/// </summary>
		int lastEndIndex = -1;

		/// <summary>
		/// notSameValue 保證 上次的尾與這次的頭不會相同
		/// </summary>
		/// <returns>The suffle.</returns>
		/// <param name="notSameValue">If set to <c>true</c> not same value.</param>
		public List<T> GetSuffle (bool notSameValue = true)
		{
			List<int> indexs = RandomTool.GetRange (datas.Count);

			var pairs = indexs.ConvertAll (index=>
				{
					float value = UnityEngine.Random.Range (0, 99f);
					return new RefKeyValuePair<int,float>(index, value);
				});

			pairs.Sort ((pair1, pair2) => pair1.value.CompareTo (pair2.value));

			var suffleIndexs = pairs.ConvertAll (pair => pair.key);

			if (lastEndIndex != -1 && notSameValue)
			{
				PrcoessNotSameValue (ref suffleIndexs);
			}

			lastEndIndex = suffleIndexs [suffleIndexs.Count - 1];

			List<T> suffleDatas = suffleIndexs.ConvertAll (index => datas [index]);

			return suffleDatas;
		}

		void PrcoessNotSameValue (ref List<int> suffleIndexs)
		{
            //如果只有一個, 就不用處理
            if (suffleIndexs.Count > 1)
			{
				suffleIndexs.Remove (lastEndIndex);

				int insertIndex = 0;

				//抽出來後, 隨機移到一個不為0的地方
				insertIndex = UnityEngine.Random.Range (1, datas.Count);

				suffleIndexs.Insert (insertIndex, lastEndIndex);
			}
		}

	}
}
