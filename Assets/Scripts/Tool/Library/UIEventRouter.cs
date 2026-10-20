using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kun.Tool
{
    public class UIEventRouter : MonoBehaviour , IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
	{
		List<Action<PointerEventData>> onPointerClickCallbacks = new List<Action<PointerEventData>> ();
		
		public void OnPointerClick (PointerEventData eventData)
		{
			if (onPointerClickCallbacks != null) 
			{
				onPointerClickCallbacks.ForEach (action => action.Invoke (eventData));
			}
		}

		List<Action<PointerEventData>> onPointerDownCallbacks = new List<Action<PointerEventData>> ();

		public void OnPointerDown (PointerEventData eventData)
		{
			if (onPointerDownCallbacks != null) 
			{
				onPointerDownCallbacks.ForEach (action => action.Invoke (eventData));
			}
		}

		List<Action<PointerEventData>> onPointerUpCallbacks = new List<Action<PointerEventData>> ();

		public void OnPointerUp (PointerEventData eventData)
		{
			if (onPointerUpCallbacks != null) 
			{
				onPointerUpCallbacks.ForEach (action => action.Invoke (eventData));
			}
		}

		List<Action<PointerEventData>> onPointerEnterCallbacks = new List<Action<PointerEventData>> ();

		public void OnPointerEnter (PointerEventData eventData)
		{
			if (onPointerEnterCallbacks != null) 
			{
				onPointerEnterCallbacks.ForEach (action => action.Invoke (eventData));
			}
		}

		List<Action<PointerEventData>> onPointerExitCallbacks = new List<Action<PointerEventData>> ();

		public void OnPointerExit (PointerEventData eventData)
		{
			if (onPointerExitCallbacks != null) 
			{
				onPointerExitCallbacks.ForEach (action => action.Invoke (eventData));
			}
		}

		/// <summary>
		/// 透過Runtime生成腳本 轉接unity點擊事件
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="target">Target.</param>
		/// <param name="router">Router.</param>
		public static void BindEvent (Action<PointerEventData> callback, GameObject target, UIEventType eventType)
		{
			UIEventRouter router = target.GetOrAddComponent<UIEventRouter> ();
			BindEvent (callback, router, eventType);
		}

		/// <summary>
		/// 如果先前生成過, 直接傳入參考 避免AddOrGerComponent
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="router">Router.</param>
		/// <param name="eventType">Event type.</param>
		public static void BindEvent (Action<PointerEventData> callback, UIEventRouter router, UIEventType eventType)
		{
			router.GetUIEvents (eventType).Add (callback);
		}

		List<Action<PointerEventData>> GetUIEvents (UIEventType type)
		{
			switch(type)
			{
			case UIEventType.Click:
				{
					return onPointerClickCallbacks;
				}

			case UIEventType.Down:
				{
					return onPointerDownCallbacks;
				}

			case UIEventType.Up:
				{
					return onPointerUpCallbacks;
				}

			case UIEventType.Enter:
				{
					return onPointerEnterCallbacks;
				}

			case UIEventType.Exit:
				{
					return onPointerExitCallbacks;
				}

			default:
				{
					Debug.LogError ($"not support -> {type}");
					return new List<Action<PointerEventData>> ();
				}
			}
		}
	}

	public enum UIEventType
	{
		Click,Down,Up,Enter,Exit
	}
}
