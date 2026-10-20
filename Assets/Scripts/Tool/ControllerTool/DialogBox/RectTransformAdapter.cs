using System;
using UnityEngine;

namespace Kun.Tool
{
    [Serializable]
	public class RectTransformAdapter 
	{
		RectTransform rectTransform;

		public RectTransformAdapter(RectTransform rectTransform)
		{
			this.rectTransform = rectTransform;
		}

		public float Height
		{
			get
			{
				return rectTransform.sizeDelta.y;
			}

			set
			{
				Vector2 origingDelta = rectTransform.sizeDelta;
				origingDelta.y = value;
				rectTransform.sizeDelta = origingDelta;
			}
		}

		public float Width
		{
			get
			{
				return rectTransform.sizeDelta.x;
			}

			set
			{
				Vector2 origingDelta = rectTransform.sizeDelta;
				origingDelta.x = value;
				rectTransform.sizeDelta = origingDelta;
			}
		}
	}
}