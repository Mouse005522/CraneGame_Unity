using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kun.Tool
{
    public static class CollectionTool
	{
		public static bool TryFind<T> (this List<T> datas, Func<T, bool> condition, out T target)
		{
            for (int i = 0; i < datas.Count; i++)
            {
				if (condition.Invoke (datas[i])) 
				{
					target = datas[i];
					return true;
				}
            }

			target = default;
			return false;
		}

		/// <summary>
		/// 通用的cache檢查
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TList"></typeparam>
		/// <param name="usedTable"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool CheckAndAdd<TKey, TList> (this Dictionary<TKey, List<TList>> usedTable, TKey key, TList value)
		{
			List<TList> usedValues;

			if (usedTable.TryGetValue (key, out usedValues) == false)
			{
				usedValues = new List<TList> ();

				usedTable.Add (key, usedValues);
			}

			bool exist = usedValues.Contains (value);

			if (exist == false)
			{
				usedValues.Add (value);
			}

			return exist == false;
		}

		public static bool Remove<T> (this List<T> list, Predicate<T> removePredicate)
		{	
			for (int i = 0; i < list.Count; i++) 
			{
				if (removePredicate.Invoke (list [i])) 
				{
					list.RemoveAt (i);
					return true;
				}
			}

			return false;
		}

		public static void Map<T> (this IList<T> target, Action<int,T> mapCallback)
		{
			for (int i = 0; i < target.Count; i++) 
			{
				mapCallback (i, target [i]);
			}
		}

		/// <summary>
		/// 第三個參數為是否為最後一個item
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="mapCallback">Map callback.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void Map<T> (this IList<T> target, Action<int,T,bool> mapCallback)
		{
			int count = target.Count;

			for (int i = 0; i < count; i++) 
			{
				bool isLast = i == count - 1;

				mapCallback (i, target [i], isLast);
			}
		}

		public static List<T> RandomSort<T> (this List<T> sources)
		{
			List<RefKeyValuePair<T,float>> pairs = sources.ConvertAll (item=>
				{
					float value = UnityEngine.Random.Range (0,999);
					return new RefKeyValuePair<T,float>(item, value);
				});

			pairs.Sort ((a, b) => a.value.CompareTo (b.value));

			return pairs.ConvertAll (pair => pair.key);
		}

		//把指定的物件轉移到別的list
		public static void TransferItem<T> (this List<T> srcList, List<T> targetList, T targetItem)
		{
			srcList.Remove (targetItem);

			targetList.Add (targetItem);
		}

		/// <summary>
		/// 檢查兩個陣列內容是否完全相同(不論順序)
		/// </summary>
		/// <returns><c>true</c>, if full same was checked, <c>false</c> otherwise.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool CheckSameValues<T> (List<T> list1, List<T> list2)
		{
			if (list1.Count != list2.Count) 
			{
				return false;
			}

			List<T> list3 = new List<T> (list1);

			foreach (var item in list2) 
			{
				bool canRemove = list3.Remove (item);

				//無法移除代表有不同的
				if (canRemove == false) 
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// 檢查兩個陣列內容是否完全相同(順序也要完全相同)
		/// </summary>
		/// <returns><c>true</c>, if full same was checked, <c>false</c> otherwise.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool CheckFullSame<T> (List<T> list1, List<T> list2)
		{
			if (list1.Count != list2.Count)
			{
				return false;
			}

			List<T> list3 = new List<T> (list1);

			foreach (var item in list2)
			{
				bool canRemove = list3.Remove (item);

				//無法移除代表有不同的
				if (canRemove == false)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Key一樣的合併在一起 把value組成list
		/// </summary>
		/// <returns>The superimposed list.</returns>
		/// <param name="source">Source.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		/// <typeparam name="TValue">The 2nd type parameter.</typeparam>
		public static List<RefKeyValuePair<TKey,List<TValue>>> GetSuperimposedList<TKey,TValue> (this List<RefKeyValuePair<TKey,TValue>> sourcePairs) where TKey:class
		{
			List<RefKeyValuePair<TKey,List<TValue>>> result = new List<RefKeyValuePair<TKey, List<TValue>>> ();

			for (int i = 0; i < sourcePairs.Count; i++)
			{
				var sourcePair = sourcePairs[i];

				RefKeyValuePair<TKey,List<TValue>> pairFinder = null;

				for (int j = 0; j < result.Count; j++)
				{
					if (result[j].key == sourcePair.key)
					{
						pairFinder = result[j];
						break;
					}
				}

				if (pairFinder == null)
				{
					pairFinder = new RefKeyValuePair<TKey,List<TValue>> (sourcePair.key);
					pairFinder.value = new List<TValue> ();

					result.Add (pairFinder);
				}

				pairFinder.value.Add (sourcePair.value);
			}

			return result;
		}

		/// <summary>
		/// 以某個長度為分界拆開
		/// </summary>
		/// <returns>The split list.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<List<T>> GetSplitList<T> (this List<T> src, int count)
		{
			int splitCount = src.Count / count;

			List<List<T>> splitList = new List<List<T>> ();

			for (int i = 0; i < splitCount; i++) 
			{
				var split = src.GetRange (count * i, count);
				splitList.Add (split);
			}

			//剩下餘數的部分
			int remaining = src.Count % count;

			if (remaining > 0) 
			{
				var remainingSplit = src.GetRange (count * splitCount, remaining);
				splitList.Add (remainingSplit);
			}

			return splitList;
		}

		/// <summary>
		/// 透過callback生成新的key,組成新的pair
		/// </summary>
		/// <returns>The pairs.</returns>
		/// <param name="sources">Sources.</param>
		/// <param name="getPairKey">Get pair key.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		/// <typeparam name="TValue">The 2nd type parameter.</typeparam>
		public static List<RefKeyValuePair<TKey,TValue>> GetPairs<TKey,TValue> (this List<TValue> sources, Func<TValue,TKey> getPairKey)
		{
			List<RefKeyValuePair<TKey,TValue>> pairs = new List<RefKeyValuePair<TKey, TValue>> ();

			for (int i = 0; i < sources.Count; i++)
			{
				TValue source = sources[i];
				TKey key = getPairKey.Invoke (source);

				pairs.Add (new RefKeyValuePair<TKey, TValue> (key, source));
			}

			return pairs;
		}

		/// <summary>
		/// 抓取最後一個物件
		/// </summary>
		/// <returns>The last.</returns>
		/// <param name="iList">I list.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T GetLast<T>(this IList<T> iList)
		{	
			int count = iList.Count;

			if (count == 0) 
			{
				throw new Exception ("長度為0 無法回傳");
			}
			else
			{
				return iList [count - 1];
			}
		}

		public static void ForEach<TKey,TValue>(this Dictionary<TKey,TValue> dict,Action<TValue> callback)
		{
			foreach (var item in dict) 
			{
				callback.Invoke (item.Value);
			}
		}

		public static void ForEach<TKey,TValue>(this Dictionary<TKey,TValue> dict,Action<TKey,TValue> callback)
		{
			foreach (var item in dict) 
			{
				callback.Invoke (item.Key, item.Value);
			}
		}

		public static List<TKey> GetAllKeys<TKey,TValue> (this Dictionary<TKey,TValue> dict)
		{
			return new List<TKey> (dict.Keys);
		}

		/// <summary>
		/// 不存在相同的值才加入
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="input">Input.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void CheckAdd<T> (this List<T> source, T input)
		{
			if (source.Contains (input) == false)
			{
				source.Add (input);
			}
		}

		/// <summary>
		/// 不存在相同的值才加入
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="input">Input.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void CheckAdd<Tkey, TValue> (this Dictionary<Tkey, TValue> source, Tkey inputKey, TValue inputValue)
		{
			if (source.ContainsKey (inputKey) == false)
			{
				source.Add (inputKey, inputValue);
			}
		}

		/// <summary>
		/// 不存在相同的值才加入
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="input">Input.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void CheckAddRange<T> (this List<T> source, IEnumerable<T> input)
		{
			foreach (var item in input)
			{
				source.CheckAdd (item);
			}
		}

		/// <summary>
		/// 讓此物件與下一個物件對調
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="moveIndex">Move index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void MoveIndexToNext<T> (this List<T> refence, int index)
		{
			//要往後移一位 所以要多準備保留一個index
			if (index > refence.Count - 2)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				MoveIndex (refence, index, +1);
			}
		}

		/// <summary>
		/// 讓此物件移到最後一位
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void MoveIndexToLast<T> (this List<T> refence, int index)
		{
			if (index > refence.Count - 1)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				T cache = refence [index];

				refence.RemoveAt (index);

				refence.Add (cache);
			}
		}

		/// <summary>
		/// 讓此物件與上一個物件對調
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="moveIndex">Move index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void MoveIndexToPrev<T> (this List<T> refence, int index)
		{
			//0不能往前移
			if (index > refence.Count - 1 || index == 0)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				MoveIndex (refence, index, -1);
			}
		}

		/// <summary>
		/// 讓此物件移到第一位
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void MoveIndexToFirst<T> (this List<T> refence, int index)
		{
			if (index > refence.Count - 1)
			{
				Debug.LogError ("out of range , count -> {refence.Count}, index -> {index}");
			}
			else
			{
				T cache = refence [index];

				refence.RemoveAt (index);

				refence.Insert (0, cache);
			}
		}


		/// <summary>
		/// 與指定的index對調內容
		/// </summary>
		/// <param name="refence">Refence.</param>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		static void MoveIndex<T> (this List<T> refence, int index, int deltaIndex)
		{
			int targetIndex = index + deltaIndex;

			T temp = refence [targetIndex];

			refence [targetIndex] = refence [index];

			refence [index] = temp;
		}

		public static void Add<TKey,TValue> (this List<RefKeyValuePair<TKey,TValue>> collection, TKey key, TValue value)
		{
			collection.Add (new RefKeyValuePair<TKey, TValue>(key, value));
		}

		public static void Add<TKey,TValue1,TValue2> (this List<RefKeyValuePair<TKey,TValue1, TValue2>> collection, TKey key, TValue1 value1, TValue2 value2)
		{
			collection.Add (new RefKeyValuePair<TKey, TValue1, TValue2> (key, value1, value2));
		}

		/// <summary>
		/// 仿照Dict, 透過key去Remove
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		/// <typeparam name="TValue">The 2nd type parameter.</typeparam>
		public static void Remove<TKey,TValue> (this List<RefKeyValuePair<TKey,TValue>> collection, TKey key)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				if (collection[i].key.Equals (key))
				{
					collection.RemoveAt (i);
					return;
				}
			}
		}

		/// <summary>
		/// 仿照Dict, 透過key去Remove
		/// list的Key是另一個RefKeyValuePair
		/// </summary>
		/// <param name="collection">Collection.</param>
		/// <param name="key">Key.</param>
		/// <typeparam name="TKey">The 1st type parameter.</typeparam>
		/// <typeparam name="TValue">The 2nd type parameter.</typeparam>
		public static void Remove<TKey,TValue1,TValue2> (this List<RefKeyValuePair<RefKeyValuePair<TKey,TValue1>,TValue2>> collection, TKey key)
		{
			for (int i = 0; i < collection.Count; i++)
			{
				if (collection[i].key.key.Equals (key))
				{
					collection.RemoveAt (i);
					return;
				}
			}
		}

		/// <summary>
		/// 拆成成對的兩個list
		/// </summary>
		/// <typeparam name="T1"></typeparam>
		/// <typeparam name="T2"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static (List<T1> keys, List<T2> values) GetMappingTable<T1, T2> (this List<(T1 key, T2 value)> src) 
		{
			List<T1> keys = new List<T1> ();
			List<T2> values = new List<T2> ();

			for (int i = 0; i < src.Count; i++)
			{
				keys.Add (src[i].key);
				values.Add (src[i].value);
			}

			return (keys, values);
		}
	}

	/// <summary>
	/// 本來的keyValuePair是 valueType 簡而言之就是refenceType
	/// </summary>
	[Serializable]
	public class RefKeyValuePair<TKey, TValue>
	{
		public TKey key;
		public TValue value;

		public RefKeyValuePair ()
		{

		}

		public RefKeyValuePair (TKey key)
		{
			this.key = key;
		}

		public RefKeyValuePair (TKey key, TValue value)
		{
			this.key = key;
			this.value = value;
		}
	}

	/// <summary>
	/// 本來的keyValuePair是 valueType 簡而言之就是refenceType
	/// </summary>
	[Serializable]
	public class RefKeyValuePair<TKey, TValue1,TValue2>
	{
		public TKey key;
		public TValue1 value1;
		public TValue2 value2;

		public RefKeyValuePair ()
		{

		}

		public RefKeyValuePair (TKey key)
		{
			this.key = key;
		}

		public RefKeyValuePair (TKey key, TValue1 value1, TValue2 value2)
		{
			this.key = key;
			this.value1 = value1;
			this.value2 = value2;
		}
	}
}
