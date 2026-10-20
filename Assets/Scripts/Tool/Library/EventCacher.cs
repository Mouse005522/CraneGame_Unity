using System;
using System.Collections.Generic;

namespace Kun.Tool
{
    public class PairEventCacher<TKey,TValue> : EventCacher<RefKeyValuePair<TKey,TValue>>
	{
		/// <summary>
		/// 如果註冊事件之前, 已經觸發過了, 
		/// 就把緩存的值拿來自動觸發
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="useOldValue">If set to <c>true</c> use old value.</param>
		public void BindEvent (Action<TKey> callback, EventCacheTag eventCacheTag, bool autoUseOldValue = true)
		{
			Action<RefKeyValuePair<TKey,TValue>> _callback = (pair) => {
				callback.Invoke (pair.key);
			};

			proxyCallbackTable.Add (eventCacheTag, callback, _callback);

			base.BindEvent (_callback, eventCacheTag, autoUseOldValue);
		}

		/// <summary>
		/// 如果註冊事件之前, 已經觸發過了, 
		/// 就把緩存的值拿來自動觸發
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="useOldValue">If set to <c>true</c> use old value.</param>
		public void BindEvent (Action<TValue> callback, EventCacheTag eventCacheTag, bool autoUseOldValue = true)
		{
			Action<RefKeyValuePair<TKey,TValue>> _callback = (pair) => {
				callback.Invoke (pair.value);
			};

			proxyCallbackTable.Add (eventCacheTag, callback, _callback);

			base.BindEvent (_callback, eventCacheTag, autoUseOldValue);
		}


		/// <summary>
		/// 如果註冊事件之前, 已經觸發過了, 
		/// 就把緩存的值拿來自動觸發
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="useOldValue">If set to <c>true</c> use old value.</param>
		public void BindEvent (Action<TKey,TValue> callback, EventCacheTag eventCacheTag, bool autoUseOldValue = true)
		{
			Action<RefKeyValuePair<TKey,TValue>> _callback = (pair) => {
				callback.Invoke (pair.key, pair.value);
			};

			proxyCallbackTable.Add (eventCacheTag, callback, _callback);

			base.BindEvent (_callback, eventCacheTag, autoUseOldValue);
		}

		public void Trigger (TKey key, TValue value)
		{
			base.Trigger (new RefKeyValuePair<TKey, TValue> (key, value));
		}

		/// <summary>
		/// value使用舊值
		/// </summary>
		/// <param name="key">Key.</param>
		public void Trigger (TKey key)
		{
			var pair = value;

			if (hasValue == false)
			{
				pair = new RefKeyValuePair<TKey, TValue> ();
			}

			pair.key = key;

			base.Trigger (pair);
		}

		/// <summary>
		/// key使用舊值
		/// </summary>
		/// <param name="key">Key.</param>
		public void TriggerPairValue (TValue pairValue)
		{
			var pair = value;

			if (hasValue == false)
			{
				pair = new RefKeyValuePair<TKey, TValue> ();
			}

			pair.value = pairValue;

			base.Trigger (pair);
		}

		//obj 可能是Action<TKey>, Action<TValue>, Action<TKey,TValue>
		//class轉obj不會boxing
		//而且頻率很低, 可以無視損耗
		List<RefKeyValuePair<EventCacheTag,object,Action<RefKeyValuePair<TKey,TValue>>>> proxyCallbackTable = new List<RefKeyValuePair<EventCacheTag, object, Action<RefKeyValuePair<TKey, TValue>>>> ();

		public override void UnBindAll ()
		{
			base.UnBindAll ();

			proxyCallbackTable.Clear ();
		}

		public override void UnBindWithTag (EventCacheTag tag)
		{
			base.UnBindWithTag (tag);

			proxyCallbackTable.RemoveAll (pair => pair.key == tag);
		}

		public TKey PairKey
		{
			get
			{
				return value.key;
			}
		}

		public TValue PairValue
		{
			get
			{
				return value.value;
			}
		}
	}
	
	public class EventCacher<T> : EventCacher
	{
		/// <summary>
		/// 如果註冊事件之前, 已經觸發過了, 可以把緩存的值拿來自動觸發
		/// </summary>
		/// <value><c>true</c> if this instance has value; otherwise, <c>false</c>.</value>
		protected bool hasValue;
		public T value;

		protected List<RefKeyValuePair<EventCacheTag,Action<T>>> callbackTable = new List<RefKeyValuePair<EventCacheTag, Action<T>>> ();

		/// <summary>
		/// 如果註冊事件之前, 已經觸發過了, 
		/// 就把緩存的值拿來自動觸發
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="useOldValue">If set to <c>true</c> use old value.</param>
		public void BindEvent (Action<T> callback, EventCacheTag eventCacheTag, bool autoUseOldValue = true)
		{
			this.callbackTable.Add (eventCacheTag, callback);

			if (autoUseOldValue) 
			{
				if (hasValue) 
				{
					callback.Invoke (value);
				}
			}
		}

		public void Trigger (T value)
		{
			this.hasValue = true;
			this.value = value;

			callbackTable.ForEach (pair => pair.value.Invoke (value));
		}

		public void UnBind (Action<T> callback)
		{
			this.callbackTable.Remove (pair => pair.value == callback);
		}

		public virtual void UnBindWithTag (EventCacheTag tag)
		{
			base.UnBindWithTag (tag);
			
			this.callbackTable.RemoveAll (pair => pair.key == tag);
		}

		public virtual void UnBindAll ()
		{
			base.UnBindAll ();
			
			this.callbackTable.Clear ();
		}
	}

	public class EventCacher
	{
		public virtual void UnBindWithTag (EventCacheTag tag)
		{
			
		}

		public virtual void UnBindAll ()
		{
			
		}
	}

	public enum EventCacheTag
	{
		/// <summary>
		/// 全域的遊戲事件
		/// </summary>
		Service,
		/// <summary>
		/// 遊玩流程進行中的流程
		/// </summary>
		GamePlay
	}
}
