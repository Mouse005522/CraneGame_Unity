using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kun.Tool
{
    public class CacheStackController<T> where T:Enum
    {
        List<StatusCacheable<T>> statusCacheables = new List<StatusCacheable<T>> ();

		Stack<T> eventStack = new Stack<T> ();

		public CacheStackController (List< StatusCacheable<T>> cacheables) 
		{
			this.statusCacheables = cacheables.ToList ();
		}

		public void SetStack (T eventType, bool isEnter)
		{
			//檢查時序是否正常
			bool successful = ProcessCursorEventStackModify (eventType, isEnter);

			if (successful)
			{
				if (isEnter)
				{
					statusCacheables.ForEach (statusCacheable => statusCacheable.PushCache (eventType));
				}
				else
				{
					statusCacheables.ForEach (statusCacheable => statusCacheable.PopCache (eventType));
				}
			}
		}

		bool ProcessCursorEventStackModify (T eventType, bool isEnter)
		{
			if (isEnter)
			{
				if (eventStack.Contains (eventType) == false)
				{
					eventStack.Push (eventType);

					return true;
				}
				else
				{
					Debug.LogErrorFormat ("has same push -> {0}", eventType);

					return false;
				}
			}
			else
			{
				if (eventStack.Peek ().Equals (eventType))
				{
					eventStack.Pop ();

					return true;
				}
				else
				{
					Debug.LogErrorFormat ("has Pop fail -> {0}", eventType);

					return false;
				}
			}
		}
	}

    public interface StatusCacheable<T> where T : Enum
    {
		/// <summary>
		/// 存入Cache
		/// </summary>
		/// <param name="eventType">Event type.</param>
		public void PushCache (T eventType);

        /// <summary>
        /// 取出
        /// </summary>
        /// <param name="eventType">Event type.</param>
        public void PopCache (T eventType);
	}
}
